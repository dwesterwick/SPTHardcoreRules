using HardcoreRules.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class TraderOffersUtil
    {
        private LoggingUtil _loggingUtil;
        private ConfigUtil _configUtil;
        private DatabaseService _databaseService;
        private ToggleHardcoreRulesService _toggleHardcoreRulesService;
        private ItemHelper _itemHelper;
        private RagfairOfferGenerator _ragfairOfferGenerator;
        private RagfairOfferService _ragfairOfferService;

        private HashSet<MongoId> _money_Ids;
        private MongoId _gpCoin_Id;

        public bool IsMoney(TemplateItem item) => IsMoney(item.Id);
        public bool IsMoney(MongoId itemId) => _money_Ids.Contains(itemId);
        
        public TraderOffersUtil
        (
            LoggingUtil loggingUtil,
            ConfigUtil configUtil,
            DatabaseService databaseService,
            ToggleHardcoreRulesService toggleHardcoreRulesService,
            ItemHelper itemHelper,
            RagfairOfferGenerator ragfairOfferGenerator,
            RagfairOfferService ragfairOfferService
        )
        {
            _loggingUtil = loggingUtil;
            _configUtil = configUtil;
            _databaseService = databaseService;
            _toggleHardcoreRulesService = toggleHardcoreRulesService;
            _itemHelper = itemHelper;
            _ragfairOfferGenerator = ragfairOfferGenerator;
            _ragfairOfferService = ragfairOfferService;

            _money_Ids = Money.GetMoneyTpls();
            _gpCoin_Id = Money.GP;
        }

        public void RefreshFleaMarketOffers()
        {
            _loggingUtil.Info("Refreshing flea market offers...");
            _ragfairOfferGenerator.GenerateDynamicOffers();

            if (_toggleHardcoreRulesService.HardcoreRulesEnabled)
            {
                RemoveBannedFleaMarketOffers();
            }
        }

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
