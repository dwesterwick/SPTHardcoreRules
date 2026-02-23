using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Server
{
    internal class TraderOffersTests
    {
        private ISptLogger<HardcoreRules_Server> _logger;
        private LoggingUtil _loggingUtil;
        private MockConfigUtil _configUtil;

        private ModHelper _modHelper = null!;
        private ItemHelper _itemHelper = null!;
        private DatabaseService _databaseService = null!;
        private LocaleService _localeService = null!;
        private ServerLocalisationService _serverLocalisationService = null!;

        private OfferModificationUtil _offerModificationUtil;
        private MockTranslationService _translationService;
        private TraderOffersUtil _traderOffersUtil;

        [SetUp]
        public void Setup()
        {
            RunFromSptInstallDirectoryService.RunFromSptInstallDirectory(LoadSptDependencies);

            _logger = new MockLogger<HardcoreRules_Server>();
            _configUtil = new MockConfigUtil(_modHelper);
            _loggingUtil = new LoggingUtil(_logger, _configUtil);

            _offerModificationUtil = new OfferModificationUtil(_loggingUtil, _configUtil, _databaseService, _itemHelper);
            _translationService = new MockTranslationService(_loggingUtil, _configUtil, _databaseService, _localeService, _serverLocalisationService);
            _traderOffersUtil = new TraderOffersUtil(_loggingUtil, _configUtil, _databaseService, _translationService, _offerModificationUtil);
        }

        private void LoadSptDependencies()
        {
            _modHelper = DI.GetInstance().GetService<ModHelper>();
            _itemHelper = DI.GetInstance().GetService<ItemHelper>();
            _databaseService = DI.GetInstance().GetService<DatabaseService>();
            _localeService = DI.GetInstance().GetService<LocaleService>();
            _serverLocalisationService = DI.GetInstance().GetService<ServerLocalisationService>();
        }

        [Test]
        public void TradeOffersEqualsBarterSchemes()
        {
            EnableTraders();
            int allTraderOffersCount = GetAllTraderOffersCount();
            int allTraderBarterSchemesCount = GetAllBarterSchemesCount();
            Assert.True(allTraderBarterSchemesCount == allTraderOffersCount, "The number of barter schemes does not match the number of trader offers");
        }

        [Test]
        public void TraderOffersCanBeToggled()
        {
            EnableTraders();
            int notWhitelistedTraderOffersCount = GetAllNotWhitelistedTraderOffersCount();
            int cashTraderOffersCount = GetAllCashTraderOffersCount();
            Assert.NotZero(notWhitelistedTraderOffersCount, "Trader offers are only whitelisted offers");
            Assert.NotZero(cashTraderOffersCount, "Trader offers are only barter offers");

            EnableTradersBarterOnly();
            int whitelistedCashTraderOffersCount = GetAllCashWhitelistedTraderOffersCount();

        }

        private void EnableTraders()
        {
            bool modConfigBartersOnly = _configUtil.CurrentConfig.Traders.BartersOnly;
            bool modConfigWhitelistOnly = _configUtil.CurrentConfig.Traders.WhitelistOnly;

            try
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = false;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = false;

                _traderOffersUtil.RestoreTraderOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = modConfigBartersOnly;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = modConfigWhitelistOnly;
            }
        }

        private void EnableTradersBarterOnly()
        {
            bool modConfigBartersOnly = _configUtil.CurrentConfig.Traders.BartersOnly;
            bool modConfigWhitelistOnly = _configUtil.CurrentConfig.Traders.WhitelistOnly;

            try
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = true;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = false;

                _traderOffersUtil.RestoreTraderOffers();
                _traderOffersUtil.RemoveBannedTraderOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = modConfigBartersOnly;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = modConfigWhitelistOnly;
            }
        }

        private void EnableTradersWhitelistOnly()
        {
            bool modConfigBartersOnly = _configUtil.CurrentConfig.Traders.BartersOnly;
            bool modConfigWhitelistOnly = _configUtil.CurrentConfig.Traders.WhitelistOnly;

            try
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = false;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = true;

                _traderOffersUtil.RestoreTraderOffers();
                _traderOffersUtil.RemoveBannedTraderOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Traders.BartersOnly = modConfigBartersOnly;
                _configUtil.CurrentConfig.Traders.WhitelistOnly = modConfigWhitelistOnly;
            }
        }

        private IEnumerable<Trader> GetTradersWithTradeOffers()
        {
            return _databaseService.GetTraders()
                .Where(trader => trader.Value.Assort?.Items.Count() > 0)
                .Select(trader => trader.Value);
        }

        private int GetAllTraderOffersCount()
        {
            return GetTradersWithTradeOffers()
                .Select(trader => trader.Assort.Items)
                .Count();
        }

        private int GetAllNotWhitelistedTraderOffersCount()
        {
            return GetTradersWithTradeOffers()
                .Select(trader => trader.Assort.Items)
                .SelectMany(items => items
                    .Select(item => _offerModificationUtil.GetItemTemplate(item))
                    .Where(template => template != null)
                    //.Where(template => !template!.IsQuestItem())
                    .Where(template => !_offerModificationUtil.IsWhitelisted(template!))
                ).Count();
        }

        private int GetAllBarterSchemesCount()
        {
            return GetTradersWithTradeOffers()
                .Select(trader => trader.Assort.BarterScheme)
                .Count();
        }

        private int GetAllCashTraderOffersCount()
        {
            return GetTradersWithTradeOffers()
                .Select(trader => trader.Assort.BarterScheme)
                .Where(scheme => scheme
                    .Select(scheme => scheme.Value)
                    .Any(scheme => !_offerModificationUtil.IsABarterOffer(scheme))
                )
                .Count();
        }

        private int GetAllCashWhitelistedTraderOffersCount()
        {
            Item[] allWhitelistedTraderOffers = GetTradersWithTradeOffers()
                .SelectMany(trader => trader.Assort.Items)
                .Where(item => _offerModificationUtil.GetItemTemplate(item.Template) != null)
                .Where(item => _offerModificationUtil.IsWhitelisted(_offerModificationUtil.GetItemTemplate(item.Template)!))
                .ToArray();

            MongoId[] allCashTraderOfferIds = GetTradersWithTradeOffers()
               .Select(trader => trader.Assort.BarterScheme)
               .Where(scheme => scheme
                   .Select(scheme => scheme.Value)
                   .Any(scheme => !_offerModificationUtil.IsABarterOffer(scheme))
               )
               .SelectMany(scheme => scheme.Keys)
               .ToArray();

            IEnumerable<Item> whitelistedCashTraderOffers = allWhitelistedTraderOffers
                .Where(item => allCashTraderOfferIds.Contains(item.Id));

            return whitelistedCashTraderOffers.Count();
        }
    }
}
