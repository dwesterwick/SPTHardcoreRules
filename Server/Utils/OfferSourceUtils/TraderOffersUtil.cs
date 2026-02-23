using HardcoreRules.Helpers;
using HardcoreRules.Services;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class TraderOffersUtil
    {
        private TraderOfferSource TraderAssorts;

        private LoggingUtil _loggingUtil;
        private ConfigUtil _configUtil;
        private DatabaseService _databaseService;
        private TranslationService _translationService;
        private OfferModificationUtil _offerModificationUtil;

        private IEnumerable<Trader> _tradersWithOffersNotIncludingFence => _databaseService.GetTables().Traders.Values
                .NotIncludingFence()
                .WithOffers();

        public TraderOffersUtil
        (
            LoggingUtil loggingUtil,
            ConfigUtil configUtil,
            DatabaseService databaseService,
            TranslationService translationService,
            OfferModificationUtil offerModificationUtil
        )
        {
            _loggingUtil = loggingUtil;
            _configUtil = configUtil;
            _databaseService = databaseService;
            _translationService = translationService;
            _offerModificationUtil = offerModificationUtil;

            TraderAssorts = new TraderOfferSource(_loggingUtil, _databaseService, RestrictTraderOffers);
        }

        public void RestoreTraderOffers() => TraderAssorts.Enable();
        public void RemoveBannedTraderOffers() => TraderAssorts.Disable();

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

            if (_offerModificationUtil.IsWhitelisted(trader))
            {
                _loggingUtil.Info($"Skipping whitelisted trader {localizedTraderName}...");
                return;
            }

            List<Item> itemsToRemove = new();
            bool lastItemWillBeRemoved = false;

            foreach (Item item in trader.Assort.Items)
            {
                if (item.SlotId?.ToLower() != DatabaseHelpers.HIDEOUT_SLOT_ID)
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

            if (!trader.Assort.BarterScheme.TryGetValue(item.Id, out List<List<BarterScheme>>? barterSchemes) || barterSchemes == null)
            {
                _loggingUtil.Error($"Could not retrieve barter scheme for item {item.Id} for trader {localizedTraderName}");
                return false;
            }

            if (barterSchemes.Count == 0)
            {
                return false;
            }

            if (!_configUtil.CurrentConfig.Traders.BartersOnly || _offerModificationUtil.IsABarterOffer(barterSchemes))
            {
                return false;
            }

            return true;
        }

        private bool RemoveItemFromTraderOffers(Trader trader, Item item)
        {
            string localizedTraderName = _translationService.GetLocalisedTraderName(trader);

            if (item.ParentId == DatabaseHelpers.HIDEOUT_SLOT_ID)
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
            TemplateItem? template = _offerModificationUtil.GetItemTemplate(item);
            if (template == null)
            {
                _loggingUtil.Error($"Template {item.Template} for item {item.Id} is null");
                return true;
            }

            if (template.Properties?.QuestItem == true)
            {
                return true;
            }

            if (_offerModificationUtil.IsWhitelisted(template))
            {
                return true;
            }

            return false;
        }
    }
}
