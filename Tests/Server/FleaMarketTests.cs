using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Generators.Ragfair;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Ragfair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Server
{
    internal class FleaMarketTests
    {
        private ISptLogger<HardcoreRules_Server> _logger;
        private LoggingUtil _loggingUtil;
        private MockConfigUtil _configUtil;

        private ModHelper _modHelper = null!;
        private ItemHelper _itemHelper = null!;
        private TradersTable _tradersTable = null!;
        private TemplateTable _templateTable = null!;
        private GlobalTable _globalTable = null!;
        private RagfairConfig _ragfairConfig = null!;
        private RagfairOfferGenerator _ragfairOfferGenerator = null!;
        private RagfairOfferService _ragfairOfferService = null!;
        private RagfairController _ragfairController = null!;

        private OfferModificationUtil _offerModificationUtil;
        private FleaMarketOffersUtil _fleaMarketOffersUtil;

        [SetUp]
        public void Setup()
        {
            RunFromSptInstallDirectoryService.RunFromSptInstallDirectory(LoadSptDependencies);

            _logger = new MockLogger<HardcoreRules_Server>();
            _configUtil = new MockConfigUtil(_modHelper);
            _loggingUtil = new LoggingUtil(_logger, _configUtil);

            _offerModificationUtil = new OfferModificationUtil(_loggingUtil, _configUtil, _tradersTable, _templateTable, _itemHelper);
            _fleaMarketOffersUtil = new FleaMarketOffersUtil
            (
                _loggingUtil,
                _configUtil,
                _ragfairConfig,
                _globalTable,
                _tradersTable,
                _offerModificationUtil,
                _ragfairOfferGenerator,
                _ragfairOfferService,
                _ragfairController
            );
        }

        private void LoadSptDependencies()
        {
            _modHelper = DI.GetInstance().GetService<ModHelper>();
            _itemHelper = DI.GetInstance().GetService<ItemHelper>();
            _tradersTable = DI.GetInstance().GetService<TradersTable>();
            _templateTable = DI.GetInstance().GetService<TemplateTable>();
            _globalTable = DI.GetInstance().GetService<GlobalTable>();
            _ragfairConfig = DI.GetInstance().GetService<RagfairConfig>();
            _ragfairOfferGenerator = DI.GetInstance().GetService<RagfairOfferGenerator>();
            _ragfairOfferService = DI.GetInstance().GetService<RagfairOfferService>();
            _ragfairController = DI.GetInstance().GetService<RagfairController>();
        }

        [Test]
        public void FleaMarketCanBeToggled()
        {
            EnableFleaMarket();
            int nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
            Assert.NotZero(nonTraderOfferCount, "No flea market offers found for players");

            DisableFleaMarket();
            nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
            Assert.Zero(nonTraderOfferCount, "Flea market offers found for players");

            EnableFleaMarket();
            nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
            Assert.NotZero(nonTraderOfferCount, "No flea market offers found for players");
        }

        [Test]
        public void FleaMarketCanBeToggledToBarterOnly()
        {
            if (_ragfairConfig.Dynamic.Barter.ChancePercent == 0)
            {
                Assert.Pass("Barter offers are disabled for players");
            }

            EnableFleaMarket();
            int nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
            Assert.NotZero(nonTraderOfferCount, "No flea market offers found for players");
            int nonTraderCashOfferCount = GetNonTraderFleaMarketCashOfferCount();
            Assert.NotZero(nonTraderCashOfferCount, "No cash flea market offers found for players");

            EnableBarterOnlyFleaMarket();
            if (_ragfairConfig.Dynamic.Barter.ChancePercent == 0)
            {
                Assert.Pass("Barter offers are disabled for players");
            }
            else
            {
                nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
                Assert.NotZero(nonTraderOfferCount, "No flea market offers found for players");
            }
            nonTraderCashOfferCount = GetNonTraderFleaMarketCashOfferCount();
            Assert.Zero(nonTraderCashOfferCount, "Cash flea market offers found for players");

            EnableFleaMarket();
            nonTraderOfferCount = GetNonTraderFleaMarketOfferCount();
            Assert.NotZero(nonTraderOfferCount, "No flea market offers found for players");
            nonTraderCashOfferCount = GetNonTraderFleaMarketCashOfferCount();
            Assert.NotZero(nonTraderCashOfferCount, "No cash flea market offers found for players");
        }

        private void EnableFleaMarket()
        {
            bool modConfigFleaMarketEnabled = _configUtil.CurrentConfig.Services.FleaMarket.Enabled;
            bool modConfigOnlyBarterOffers = _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers;

            try
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = true;
                _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers = false;

                _fleaMarketOffersUtil.EnableFleaMarket();
                RefreshFleaMarketAndRemoveBannedOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = modConfigFleaMarketEnabled;
                _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers = modConfigOnlyBarterOffers;
            }
        }

        private void EnableBarterOnlyFleaMarket()
        {
            bool modConfigFleaMarketEnabled = _configUtil.CurrentConfig.Services.FleaMarket.Enabled;
            bool modConfigOnlyBarterOffers = _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers;

            try
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = true;
                _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers = true;

                _fleaMarketOffersUtil.EnableFleaMarket();
                RefreshFleaMarketAndRemoveBannedOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = modConfigFleaMarketEnabled;
                _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers = modConfigOnlyBarterOffers;
            }
        }

        private void DisableFleaMarket()
        {
            bool modConfigFleaMarketEnabled = _configUtil.CurrentConfig.Services.FleaMarket.Enabled;

            try
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = false;

                _fleaMarketOffersUtil.DisableFleaMarket();
                RefreshFleaMarketAndRemoveBannedOffers();
            }
            finally
            {
                _configUtil.CurrentConfig.Services.FleaMarket.Enabled = modConfigFleaMarketEnabled;
            }
        }

        private void RefreshFleaMarket() => _fleaMarketOffersUtil.RefreshFleaMarketOffers();

        private void RefreshFleaMarketAndRemoveBannedOffers()
        {
            RefreshFleaMarket();
            _fleaMarketOffersUtil.RemoveBannedFleaMarketOffers();
        }

        private int GetNonTraderFleaMarketCashOfferCount() => GetNonTraderFleaMarketCashOffers().Count();

        private IEnumerable<RagfairOffer> GetNonTraderFleaMarketCashOffers()
        {
            IEnumerable<RagfairOffer> allOffers = GetNonTraderFleaMarketOffers();
            return allOffers.Where(offer => !_offerModificationUtil.IsABarterOffer(offer));
        }

        private int GetNonTraderFleaMarketOfferCount() => GetNonTraderFleaMarketOffers().Count();

        private IEnumerable<RagfairOffer> GetNonTraderFleaMarketOffers()
        {
            List<RagfairOffer> offers = _ragfairOfferService.GetOffers();
            return offers.Where(offer => offer.User?.MemberType != SPTarkov.Server.Core.Models.Enums.MemberCategory.Trader);
        }
    }
}
