using HardcoreRules.Server.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.Server.Core.Helpers;
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
        public void Test1()
        {
            Assert.Pass();
        }
    }
}
