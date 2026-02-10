using HardcoreRules.Helpers;
using HardcoreRules.Services;
using HardcoreRules.Utils.OfferRequirementWrappers;
using HardcoreRules.Utils.OfferRequirementWrappers.Internal;
using HardcoreRules.Utils.TraderOfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils
{
    [Injectable(InjectionType.Singleton)]
    internal class TraderOffersUtil
    {
        private const string HIDEOUT_SLOT_ID = "hideout";

        private FleaMarketOfferSource FleaMarket;
        private FenceOfferSource Fence;
        private GiftsOfferSource Gifts;
        private TraderOfferSource TraderAssorts;

        private LoggingUtil _loggingUtil;
        private ConfigUtil _configUtil;
        private TranslationService _translationService;
        private ConfigServer _configServer;
        private DatabaseService _databaseService;
        private ItemHelper _itemHelper;
        private RagfairOfferGenerator _ragfairOfferGenerator;
        private RagfairOfferService _ragfairOfferService;
        private FenceService _fenceService;

        private HashSet<MongoId> _money_Ids;
        private MongoId _gpCoin_Id;

        private MongoId[] _whiteListedTraders = null!;
        public IReadOnlyCollection<MongoId> WhitelistedTraders
        {
            get
            {
                if (_whiteListedTraders == null)
                {
                    _whiteListedTraders = GetWhiteListedTraders();
                }

                return _whiteListedTraders.AsReadOnly();
            }
        }

        private MongoId[] _whiteListedItems = null!;
        public IReadOnlyCollection<MongoId> WhitelistedItems
        {
            get
            {
                if (_whiteListedItems == null)
                {
                    _whiteListedItems = GetWhiteListedItems();
                }

                return _whiteListedItems.AsReadOnly();
            }
        }

        public bool IsMoney(TemplateItem item) => IsMoney(item.Id);
        public bool IsMoney(MongoId itemId) => _money_Ids.Contains(itemId);
        
        public TraderOffersUtil
        (
            LoggingUtil loggingUtil,
            ConfigUtil configUtil,
            TranslationService translationService,
            ConfigServer configServer,
            DatabaseService databaseService,
            ItemHelper itemHelper,
            RagfairOfferGenerator ragfairOfferGenerator,
            RagfairOfferService ragfairOfferService,
            FenceService fenceService
        )
        {
            _loggingUtil = loggingUtil;
            _configUtil = configUtil;
            _translationService = translationService;
            _configServer = configServer;
            _databaseService = databaseService;
            _itemHelper = itemHelper;
            _ragfairOfferGenerator = ragfairOfferGenerator;
            _ragfairOfferService = ragfairOfferService;
            _fenceService = fenceService;

            _money_Ids = Money.GetMoneyTpls();
            _gpCoin_Id = Money.GP;

            FleaMarket = new FleaMarketOfferSource(_loggingUtil, _configServer, _databaseService, _ragfairOfferGenerator);
            Fence = new FenceOfferSource(_loggingUtil, _configServer, _fenceService);
            Gifts = new GiftsOfferSource(_loggingUtil, _configServer);
            TraderAssorts = new TraderOfferSource(_loggingUtil, _databaseService, RestrictTraderOffers);
        }

        private MongoId[] GetWhiteListedTraders()
        {
            return _databaseService.GetTraders()
                .Where(trader => _configUtil.CurrentConfig.Traders.WhitelistTraders.Contains(trader.Key))
                .Select(trader => trader.Key)
                .ToArray();
        }

        private MongoId[] GetWhiteListedItems()
        {
            return _databaseService.GetTables().Templates.Items
                .Where(item => _configUtil.CurrentConfig.Traders.WhitelistItems.Contains(item.Key))
                .Select(item => item.Key)
                .ToArray();
        }

        private IEnumerable<Trader> _tradersWithOffersNotIncludingFence => _databaseService.GetTables().Traders.Values
                .NotIncludingFence()
                .WithOffers();

        private void RestrictTraderOffers()
        {
            foreach (Trader trader in _tradersWithOffersNotIncludingFence)
            {
                RestrictTraderOffers(trader);
            }
        }

        private void RestrictTraderOffers(Trader trader)
        {
            string localizedTraderName = _translationService.GetLocalisedTraderName(trader);

            if (IsWhitelisted(trader))
            {
                _loggingUtil.Info($"Skipping whitelisted trader {localizedTraderName}...");
                return;
            }

            List<Item> itemsToRemove = new();
            bool lastItemWillBeRemoved = false;

            foreach (Item item in trader.Assort.Items)
            {
                if (item.SlotId?.ToLower() != HIDEOUT_SLOT_ID)
                {
                    if (lastItemWillBeRemoved)
                    {
                        itemsToRemove.Add(item);
                    }

                    continue;
                }

                lastItemWillBeRemoved = false;

                if (!ShouldRemoveItemFromTraderOffers(trader, item))
                {
                    continue;
                }

                itemsToRemove.Add(item);
                lastItemWillBeRemoved = true;
            }

            foreach (Item item in itemsToRemove)
            {
                RemoveItemFromTraderOffers(trader, item);
            }

            _loggingUtil.Info($"Removed {itemsToRemove.Count} banned offers from trader {localizedTraderName}");
        }

        private bool ShouldRemoveItemFromTraderOffers(Trader trader, Item item)
        {
            string localizedTraderName = _translationService.GetLocalisedTraderName(trader);

            if (MustKeepTraderAssortItem(item))
            {
                return false;
            }

            if (_configUtil.CurrentConfig.Traders.WhitelistOnly)
            {
                return true;
            }

            if (!trader.Assort.BarterScheme.TryGetValue(item.Id, out List<List<BarterScheme>>? barterSchemes) || (barterSchemes == null))
            {
                _loggingUtil.Error($"Could not retrieve barter scheme for item {item.Id} for trader {localizedTraderName}");
                return false;
            }

            if (barterSchemes.Count == 0)
            {
                return false;
            }

            if (!_configUtil.CurrentConfig.Traders.BartersOnly || IsABarterOffer(barterSchemes))
            {
                return false;
            }

            return true;
        }

        private bool RemoveItemFromTraderOffers(Trader trader, Item item)
        {
            string localizedTraderName = _translationService.GetLocalisedTraderName(trader);

            if (item.ParentId == HIDEOUT_SLOT_ID)
            {
                if (!trader.Assort.LoyalLevelItems.Remove(item.Id))
                {
                    _loggingUtil.Error($"Could not remove item {item.Id} from LoyalLevelItems for trader {localizedTraderName}");
                    return false;
                }

                if (!trader.Assort.BarterScheme.Remove(item.Id))
                {
                    _loggingUtil.Error($"Could not remove item {item.Id} from BarterScheme for trader {localizedTraderName}");
                    return false;
                }
            }

            if (!trader.Assort.Items.Remove(item))
            {
                _loggingUtil.Error($"Could not remove item {item.Id} from trader {localizedTraderName}'s offers");
                return false;
            }

            foreach (Dictionary<MongoId, MongoId> questAssort in trader.QuestAssort.Values)
            {
                questAssort.Remove(item.Id);
            }

            return true;
        }

        private bool MustKeepTraderAssortItem(Item item)
        {
            if (!_databaseService.GetTables().Templates.Items.TryGetValue(item.Template, out TemplateItem? template) || (template == null))
            {
                _loggingUtil.Error($"Could not retrieve template {item.Template} for item {item.Id}");
                return true;
            }

            if (template.Properties?.QuestItem == true)
            {
                return true;
            }

            if (IsWhitelisted(template))
            {
                return true;
            }

            return false;
        }

        public bool IsWhitelisted(TemplateItem template)
        {
            if (WhitelistedItems.Contains(template.Id))
            {
                return true;
            }

            if (WhitelistedItems.Any(whitelistedItem => _itemHelper.IsOfBaseclass(template.Id, whitelistedItem)))
            {
                return true;
            }

            return false;
        }

        public bool IsWhitelisted(Trader trader) => WhitelistedTraders.Contains(trader.Base.Id);

        public void RestoreTraderOffers() => TraderAssorts.Enable();
        public void RemoveBannedTraderOffers() => TraderAssorts.Disable();

        public void DisableFence() => Fence.Disable();
        public void EnableFence() => Fence.Enable();
        public void RefreshFenceOffers() => Fence.Refresh();

        public void DisableGifts() => Gifts.Disable();
        public void EnableGifts() => Gifts.Enable();

        public void DisableFleaMarket() => FleaMarket.Disable();
        public void EnableFleaMarket() => FleaMarket.Enable();
        public void RefreshFleaMarketOffers() => FleaMarket.Refresh();

        public void RefreshFleaMarketOffersAndRemoveBannedOffers()
        {
            FleaMarket.Refresh();
            RemoveBannedFleaMarketOffers();
        }

        public void RemoveBannedFleaMarketOffers()
        {
            bool onlyBarterOffers = _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers;
            _loggingUtil.Info($"Removing all {(onlyBarterOffers ? "" : "cash ")}flea market offers...");

            List<RagfairOffer> offers = _ragfairOfferService.GetOffers();
            foreach (RagfairOffer offer in offers)
            {
                if (!ShouldRemoveFleaMarketOffer(offer))
                {
                    continue;
                }

                _ragfairOfferService.RemoveOfferById(offer.Id);
            }

            CreateFleaMarketOffersForTraders();
        }

        public void CreateFleaMarketOffersForTraders()
        {
            _loggingUtil.Info("Adding flea market offers for traders...");

            foreach (Trader trader in _tradersWithOffersNotIncludingFence)
            {
                _ragfairOfferGenerator.GenerateFleaOffersForTrader(trader.Base.Id);
            }
        }

        public bool ShouldRemoveFleaMarketOffer(RagfairOffer offer)
        {
            if (!_configUtil.CurrentConfig.Services.FleaMarket.Enabled)
            {
                return true;
            }

            if (_configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers && !IsABarterOffer(offer))
            {
                return true;
            }

            return false;
        }

        public bool HasCashOffers(GetOffersResult getOffersResult)
        {
            return getOffersResult.Offers?.Any(offer => !IsABarterOffer(offer)) == true;
        }

        public bool IsABarterOffer(RagfairOffer offer) => IsABarterOffer(offer.Requirements);

        public bool IsABarterOffer(IEnumerable<OfferRequirement>? offerRequirements)
        {
            if (offerRequirements == null)
            {
                return false;
            }

            IEnumerable<FleaMarketOfferRequirement> requirements = offerRequirements.Select(req => new FleaMarketOfferRequirement(req));
            return IsABarterOffer(requirements);
        }

        public bool IsABarterOffer(IEnumerable<IEnumerable<BarterScheme>> offer)
        {
            foreach (IEnumerable<BarterScheme> offerRequirements in offer)
            {
                return IsABarterOffer(offerRequirements);
            }

            return false;
        }

        public bool IsABarterOffer(IEnumerable<BarterScheme>? offerRequirements)
        {
            if (offerRequirements == null)
            {
                return false;
            }

            IEnumerable<BarterSchemeRequirement> requirements = offerRequirements.Select(req => new BarterSchemeRequirement(req));
            return IsABarterOffer(requirements);
        }

        private bool IsABarterOffer(IEnumerable<IAbstractOfferRequirement>? offerRequirements)
        {
            if (offerRequirements == null)
            {
                return false;
            }

            foreach (IAbstractOfferRequirement requirement in offerRequirements)
            {
                if (!_databaseService.GetTables().Templates.Items.TryGetValue(requirement.Template, out TemplateItem? requiredItem) || (requiredItem == null))
                {
                    _loggingUtil.Error($"Could not get required item {requirement.Template} for barter scheme");
                    return false;
                }

                if (_configUtil.CurrentConfig.Traders.AllowGPCoins && (requiredItem.Id == _gpCoin_Id))
                {
                    return true;
                }

                if (!IsMoney(requiredItem))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
