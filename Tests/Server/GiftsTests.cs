using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Server
{
    internal class GiftsTests
    {
        private ISptLogger<HardcoreRules_Server> _logger;
        private LoggingUtil _loggingUtil;
        private MockConfigUtil _configUtil;

        private ConfigServer _configServer = null!;
        private ModHelper _modHelper = null!;
        private GiftsConfig _giftsConfig = null!;

        private GiftOffersUtil _giftOffersUtil;

        private int GiftItemsCount => _giftsConfig.Gifts.Sum(gift => gift.Value.Items.Count);

        [SetUp]
        public void Setup()
        {
            RunFromSptInstallDirectoryService.RunFromSptInstallDirectory(LoadSptDependencies);

            _logger = new MockLogger<HardcoreRules_Server>();
            _configUtil = new MockConfigUtil(_modHelper);
            _loggingUtil = new LoggingUtil(_logger, _configUtil);

            _giftOffersUtil = new GiftOffersUtil(_loggingUtil, _configServer);
        }

        private void LoadSptDependencies()
        {
            _modHelper = DI.GetInstance().GetService<ModHelper>();
            _configServer = DI.GetInstance().GetService<ConfigServer>();

            _giftsConfig = _configServer.GetConfig<GiftsConfig>();
        }

        [Test]
        public void GiftsCanBeToggled()
        {
            Assert.NotZero(GiftItemsCount, "No gifts found");

            _giftOffersUtil.DisableGifts();
            Assert.Zero(GiftItemsCount, "Gifts found");

            _giftOffersUtil.EnableGifts();
            Assert.NotZero(GiftItemsCount, "No gifts found");
        }
    }
}
