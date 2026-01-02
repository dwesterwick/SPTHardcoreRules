using HardcoreRules.Helpers;
using HardcoreRules.Services;
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
    public class TraderOffersUtil
    {
        private FleaMarketOfferSource FleaMarket;
        private FenceOfferSource Fence;
        private GiftsOfferSource Gifts;
        private TraderOfferSource Traders;

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
            Traders = new TraderOfferSource(_loggingUtil, _databaseService, RestrictTraderOffers);
        }

        private MongoId[] GetWhiteListedTraders()
        {
            return _databaseService.GetTraders()
                .Where(trader => _configUtil.CurrentConfig.Traders.WhitelistTraders.Contains(trader.Key))
                .Select(trader => trader.Key)
                .ToArray();
        }

        private void RestrictTraderOffers()
        {
            foreach ((MongoId id, Trader trader) in _databaseService.GetTables().Traders)
            {
                if (WhitelistedTraders.Contains(id))
                {
                    _loggingUtil.Info($"Skipping whitelisted trader {_translationService.GetLocalisedTraderName(trader)}...");
                    continue;
                }

                _loggingUtil.Info($"Removing banned offers from trader {_translationService.GetLocalisedTraderName(trader)}...");


            }
        }

        public void RestoreTraderOffers() => Traders.Enable();
        public void RemoveBannedTraderOffers() => Traders.Disable();

        public void DisableFence() => Fence.Disable();
        public void EnableFence() => Fence.Enable();
        public void RefreshFenceOffers() => Fence.Refresh();

        public void DisableGifts() => Gifts.Disable();
        public void EnableGifts() => Gifts.Enable();

        public void DisableFleaMarket() => FleaMarket.Disable();
        public void EnableFleaMarket() => FleaMarket.Enable();
        public void RefreshFleaMarketOffers() => FleaMarket.Refresh();

        public void RemoveBannedFleaMarketOffers()
        {
            bool onlyBarterOffers = _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers;
            _loggingUtil.Info($"Removing {(onlyBarterOffers ? "" : "cash")} offers from players...");

            List<RagfairOffer> offers = _ragfairOfferService.GetOffers();
            foreach (RagfairOffer offer in offers)
            {
                if (!ShouldRemoveFleaMarketOffer(offer))
                {
                    continue;
                }

                _ragfairOfferService.RemoveOfferById(offer.Id);
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

            foreach (OfferRequirement requirement in offerRequirements)
            {
                if (!_databaseService.GetTables().Templates.Items.TryGetValue(requirement.TemplateId, out TemplateItem? requiredItem) || (requiredItem == null))
                {
                    _loggingUtil.Error($"Could not get required item {requirement.TemplateId} for barter scheme");
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
