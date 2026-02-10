using HardcoreRules.Utils;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers.Internal
{
    public abstract class AbstractDynamicRouter : AbstractTypedDynamicRouter<EmptyRequestData>
    {
        public AbstractDynamicRouter(IEnumerable<string> _routeNames, LoggingUtil logger, ConfigUtil config, JsonUtil jsonUtil)
            : base(_routeNames, logger, config, jsonUtil)
        {

        }
    }
}
