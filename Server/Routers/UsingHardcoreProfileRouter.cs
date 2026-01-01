using HardcoreRules.Helpers;
using HardcoreRules.Routers.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    public class UsingHardcoreProfileRouter : AbstractStaticRouter
    {
        private static readonly string[] _routeNames = ["IsUsingHardcoreProfile"];

        private ProfileUtil _profileUtil = null!;

        public UsingHardcoreProfileRouter
        (
            LoggingUtil logger,
            ConfigUtil config,
            JsonUtil jsonUtil,
            ProfileUtil profileUtil
        ) : base(_routeNames, logger, config, jsonUtil)
        {
            _profileUtil = profileUtil;
        }

        public override bool ShouldCreateRoutes() => true;

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            bool isUsingAHardcoreProfile = _profileUtil.IsUsingAHardcoreProfile(routerData.SessionId);
            bool useHardcoreRules = isUsingAHardcoreProfile || Config.CurrentConfig.UseForAllProfiles;

            return JsonUtil.SerializeToValueTask(useHardcoreRules);
        }
    }
}
