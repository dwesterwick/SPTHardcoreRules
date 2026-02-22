using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HardcoreRules.Server
{
    internal class FenceTests
    {
        private ISptLogger<HardcoreRules_Server> _logger;
        private LoggingUtil _loggingUtil;
        private MockConfigUtil _configUtil;

        private ConfigServer _configServer = null!;
        private ModHelper _modHelper = null!;
        private FenceService _fenceService = null!;
        private SaveServer _saveServer = null!;
        private ProfileHelper _profileHelper = null!;

        private MongoId sessionId;
        private FenceOffersUtil _fenceOffersUtil = null!;

        [SetUp]
        public void Setup()
        {
            SptDependencyLoader.LoadDependencies(LoadSptDependencies);

            _logger = new MockLogger<HardcoreRules_Server>();
            _configUtil = new MockConfigUtil(_modHelper);
            _loggingUtil = new LoggingUtil(_logger, _configUtil);

            _fenceOffersUtil = new FenceOffersUtil(_loggingUtil, _configServer, _fenceService);

            CreateMockSptProfile();
        }

        private void CreateMockSptProfile()
        {
            sessionId = new MongoId();
            SPTarkov.Server.Core.Models.Eft.Profile.Info profileInfo = new SPTarkov.Server.Core.Models.Eft.Profile.Info() { ProfileId = sessionId };
            _saveServer.CreateProfile(profileInfo);

            PmcData? pmcData = _profileHelper.GetPmcProfile(sessionId);
            if (pmcData == null)
            {
                throw new InvalidOperationException("Could not create PMC data in mock SPT profile");
            }

            TraderInfo fenceInfo = new TraderInfo() { Standing = 0 };
            pmcData.TradersInfo = new Dictionary<MongoId, TraderInfo>()
            {
                { Traders.FENCE, fenceInfo },
            };
        }

        private void LoadSptDependencies()
        {
            _modHelper = DI.GetInstance().GetService<ModHelper>();
            _configServer = DI.GetInstance().GetService<ConfigServer>();
            _fenceService = DI.GetInstance().GetService<FenceService>();
            _saveServer = DI.GetInstance().GetService<SaveServer>();
            _profileHelper = DI.GetInstance().GetService<ProfileHelper>();
        }

        [Test]
        public void FenceOffersCanBeToggled()
        {
            PmcData? pmcData = _profileHelper.GetPmcProfile(sessionId);
            Assert.NotNull("Could not retrieve PMC data for mock SPT profile");

            TraderAssort fenceAssort = GetFenceAssort(pmcData!);
            Assert.NotZero(fenceAssort.Items.Count, "Fence does not have any offers");

            _fenceOffersUtil.DisableFence();
            fenceAssort = GetFenceAssort(pmcData!);
            Assert.Zero(fenceAssort.Items.Count, "Fence has offers");

            _fenceOffersUtil.EnableFence();
            fenceAssort = GetFenceAssort(pmcData!);
            Assert.NotZero(fenceAssort.Items.Count, "Fence does not have any offers");
        }

        private TraderAssort GetFenceAssort(PmcData pmcData)
        {
            _fenceOffersUtil.RefreshFenceOffers();
            return _fenceService.GetFenceAssorts(pmcData);
        }
    }
}
