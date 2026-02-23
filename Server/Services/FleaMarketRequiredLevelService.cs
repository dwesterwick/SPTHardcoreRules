using HardcoreRules.Helpers;
using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    internal class FleaMarketRequiredLevelService : AbstractService
    {
        private DatabaseService _databaseService;

        public FleaMarketRequiredLevelService(LoggingUtil logger, ConfigUtil config, DatabaseService databaseService) : base(logger, config)
        {
            _databaseService = databaseService;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            if (!Config.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            int minLevel = Config.CurrentConfig.Debug.FleaMarketMinLevel;
            _databaseService.GetTables().Globals.Configuration.RagFair.MinUserLevel = minLevel;
            Logger.Info($"Set required player level for flea-market access to {minLevel}");
        }
    }
}
