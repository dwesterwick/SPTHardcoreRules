using HardcoreRules.Routers.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    public class ConfigRouter : AbstractStaticRouter
    {
        private static readonly string[] _routeNames = [ "GetConfig" ];

        public ConfigRouter(LoggingUtil logger, ConfigUtil config, JsonUtil jsonUtil) : base(_routeNames, logger, config, jsonUtil)
        {

        }

        public override bool ShouldCreateRoutes() => true;

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            string json = ConfigUtil.Serialize(Config.CurrentConfig);
            return new ValueTask<string?>(json);
        }
    }
}
