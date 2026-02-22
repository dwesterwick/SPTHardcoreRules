using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Server.Core.Helpers;
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
    internal class OfferModificationTests
    {
        private const string ID_ITEM_SPECIAL = "5447e0e74bdc2d3c308b4567";
        private const string ID_ITEM_MULTITOOL = "544fb5454bdc2df8738b456a";
        private const string ID_ITEM_LEGA = "6656560053eaaa7a23349c86";
        private const string ID_ITEM_SHATUNS_KEY = "664d3db6db5dea2bad286955";

        private const string ID_TRADER_MECHANIC = "5a7c2eca46aef81a7ca2145d";
        private const string ID_OFFER_MECHANIC_TOOLSET = "686e34386c2a18ed6b0e9c3c";
        private const string ID_OFFER_MECHANIC_MULTITOOL = "686e342e6c2a18ed6b0e983a";

        private const string ID_TRADER_REF = "6617beeaa9cfa777ca915b7c";
        private const string ID_OFFER_REF_ARMOR_REPAIR_KIT = "68a58a62b20845b9d00bbf4b";
        private const string ID_OFFER_REF_SHATUNS_KEY = "68a58a60b20845b9d00bbe30";

        private ISptLogger<HardcoreRules_Server> _logger;
        private LoggingUtil _loggingUtil; 
        private MockConfigUtil _configUtil;

        private DatabaseService _databaseService = null!;
        private ItemHelper _itemHelper = null!;
        private ModHelper _modHelper = null!;

        private OfferModificationUtil _offerModificationUtil = null!;

        [SetUp]
        public void Setup()
        {
            SptDependencyLoader.LoadDependencies(LoadSptDependencies);

            _logger = new MockLogger<HardcoreRules_Server>();
            _configUtil = new MockConfigUtil(_modHelper);
            _loggingUtil = new LoggingUtil(_logger, _configUtil);

            _offerModificationUtil = new OfferModificationUtil(_loggingUtil, _configUtil, _databaseService, _itemHelper);
        }

        private void LoadSptDependencies()
        {
            _databaseService = DI.GetInstance().GetService<DatabaseService>();
            _itemHelper = DI.GetInstance().GetService<ItemHelper>();
            _modHelper = DI.GetInstance().GetService<ModHelper>();
        }

        [Test]
        public void ToolsetFromMechanicIsBarterTrade()
        {
            Trader? mechanic = _databaseService.GetTrader(ID_TRADER_MECHANIC);
            Assert.NotNull(mechanic, "Cannot find trader Mechanic");

            bool foundToolsetBarter = mechanic.Assort.BarterScheme.TryGetValue(ID_OFFER_MECHANIC_TOOLSET, out List<List<BarterScheme>>? requirements);
            Assert.True(foundToolsetBarter, "Cannot find Mechanic's toolset offer");
            Assert.NotNull(requirements, "Barter requirements for Mechanic's toolset offer are null");

            bool requirementsAreAllBarterItems = _offerModificationUtil.IsABarterOffer(requirements);
            Assert.True(requirementsAreAllBarterItems, "At least one of the requirements for Mechanic's toolset offer is not a barter item");
        }

        [Test]
        public void MultitoolFromMechanicIsCashTrade()
        {
            Trader? mechanic = _databaseService.GetTrader(ID_TRADER_MECHANIC);
            Assert.NotNull(mechanic, "Cannot find trader Mechanic");

            bool foundToolsetBarter = mechanic.Assort.BarterScheme.TryGetValue(ID_OFFER_MECHANIC_MULTITOOL, out List<List<BarterScheme>>? requirements);
            Assert.True(foundToolsetBarter, "Cannot find Mechanic's multitool offer");
            Assert.NotNull(requirements, "Barter requirements for Mechanic's multitool offer are null");

            bool requirementsAreAllBarterItems = _offerModificationUtil.IsABarterOffer(requirements);
            Assert.False(requirementsAreAllBarterItems, "Mechanic's multitool offer is a barter trade");
        }

        [Test]
        public void MultitoolsAreCorrectlyWhitelistedByModConfig()
        {
            bool whitelistContainsSpecialItems = _configUtil.CurrentConfig.Traders.WhitelistItems.Contains(ID_ITEM_SPECIAL);

            bool foundMultitool = _databaseService.GetItems().TryGetValue(ID_ITEM_MULTITOOL, out TemplateItem? item);
            Assert.True(foundMultitool, "Could not find the multitool item");
            Assert.NotNull(item, "The multitool item template is null");

            bool isItemWhitelisted = _offerModificationUtil.IsWhitelisted(item);

            Assert.False(whitelistContainsSpecialItems ^ isItemWhitelisted, "Multitool item whitelisting status does not follow mod trader whitelist");
        }

        [Test]
        public void ArmorRepairKitFromRefIsCorrectlyMarkedAsBarterTrade()
        {
            bool gpCoinsAllowed = _configUtil.CurrentConfig.Traders.AllowGPCoins;

            Trader? traderRef = _databaseService.GetTrader(ID_TRADER_REF);
            Assert.NotNull(traderRef, "Cannot find trader Ref");

            bool foundArmorRepairKitOffer = traderRef.Assort.BarterScheme.TryGetValue(ID_OFFER_REF_ARMOR_REPAIR_KIT, out List<List<BarterScheme>>? requirements);
            Assert.True(foundArmorRepairKitOffer, "Cannot find Ref's armor repair kit offer");
            Assert.NotNull(requirements, "Barter requirements for Ref's armor repair kit offer are null");

            bool requirementsAreAllBarterItems = _offerModificationUtil.IsABarterOffer(requirements);
            Assert.False(gpCoinsAllowed ^ requirementsAreAllBarterItems, "Ref's armor repair kit offer is not classified as a barter trade correctly per the mod config");

            try
            {
                _configUtil.CurrentConfig.Traders.AllowGPCoins = !gpCoinsAllowed;
                bool gpCoinsAllowedTmp = _configUtil.CurrentConfig.Traders.AllowGPCoins;

                requirementsAreAllBarterItems = _offerModificationUtil.IsABarterOffer(requirements);
                Assert.False(gpCoinsAllowedTmp ^ requirementsAreAllBarterItems, "Ref's armor repair kit offer is not classified as a barter trade correctly per the mod config (opposite setting)");
            }
            finally
            {
                _configUtil.CurrentConfig.Traders.AllowGPCoins = gpCoinsAllowed;
            }
        }

        [Test]
        public void ShatunsKeyFromRefIsBarterTrade()
        {
            Trader? traderRef = _databaseService.GetTrader(ID_TRADER_REF);
            Assert.NotNull(traderRef, "Cannot find trader Ref");

            bool shatunsKeyOffer = traderRef.Assort.BarterScheme.TryGetValue(ID_OFFER_REF_SHATUNS_KEY, out List<List<BarterScheme>>? requirements);
            Assert.True(shatunsKeyOffer, "Cannot find Ref's offer for Shatun's key");
            Assert.NotNull(requirements, "Barter requirements for Ref's Shatun's key offer are null");

            bool requirementsAreAllBarterItems = _offerModificationUtil.IsABarterOffer(requirements);
            Assert.True(requirementsAreAllBarterItems, "At least one of the requirements for Ref's Shatun's key offer is not a barter item");
        }
    }
}
