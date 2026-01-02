using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostSptModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    public class ToggleHardcoreRulesService : AbstractService
    {
        public bool HardcoreRulesEnabled { get; private set; } = false;

        private DatabaseService _databaseService;

        public ToggleHardcoreRulesService(LoggingUtil logger, ConfigUtil config, DatabaseService databaseService) : base(logger, config)
        {
            _databaseService = databaseService;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            
        }

        public void ToggleHardcoreRules(bool enableHardcoreRules)
        {

        }
    }
}
