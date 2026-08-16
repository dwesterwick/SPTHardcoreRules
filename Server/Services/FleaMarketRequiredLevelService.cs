using HardcoreRules.Helpers;
using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.Preload + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    internal class FleaMarketRequiredLevelService : AbstractService
    {
        private GlobalTable _globalTable;

        public FleaMarketRequiredLevelService(LoggingUtil logger, ConfigUtil config, GlobalTable globalTable) : base(logger, config)
        {
            _globalTable = globalTable;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            if (!Config.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            int minLevel = Config.CurrentConfig.Debug.FleaMarketMinLevel;
            _globalTable.Configuration.RagFair.MinUserLevel = minLevel;
            Logger.Info($"Set required player level for flea-market access to {minLevel}");
        }
    }
}
