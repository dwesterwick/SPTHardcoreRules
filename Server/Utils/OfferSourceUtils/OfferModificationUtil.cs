using HardcoreRules.Helpers;
using HardcoreRules.Utils.OfferRequirementWrappers;
using HardcoreRules.Utils.OfferRequirementWrappers.Internal;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class OfferModificationUtil
    {
        private LoggingUtil _loggingUtil;
        private ConfigUtil _configUtil;
        private DatabaseService _databaseService;
        private ItemHelper _itemHelper;

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
        
        public OfferModificationUtil
        (
            LoggingUtil loggingUtil,
            ConfigUtil configUtil,
            DatabaseService databaseService,
            ItemHelper itemHelper
        )
        {
            _loggingUtil = loggingUtil;
            _configUtil = configUtil;
            _databaseService = databaseService;
            _itemHelper = itemHelper;

            _money_Ids = Money.GetMoneyTpls();
            _gpCoin_Id = Money.GP;
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
                if (!_databaseService.GetTables().Templates.Items.TryGetValue(requirement.Template, out TemplateItem? requiredItem) || requiredItem == null)
                {
                    _loggingUtil.Error($"Could not get required item {requirement.Template} for barter scheme");
                    return false;
                }

                if (_configUtil.CurrentConfig.Traders.AllowGPCoins && requiredItem.Id == _gpCoin_Id)
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
