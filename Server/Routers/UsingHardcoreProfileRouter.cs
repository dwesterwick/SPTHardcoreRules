using HardcoreRules.Helpers;
using HardcoreRules.Routers.Internal;
using HardcoreRules.Services;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    public class UsingHardcoreProfileRouter : AbstractStaticRouter
    {
        private static readonly string[] _routeNames = ["IsUsingHardcoreProfile"];

        private AddHardcoreProfileService _addHardcoreProfileService = null!;

        public UsingHardcoreProfileRouter
        (
            LoggingUtil logger,
            ConfigUtil config,
            JsonUtil jsonUtil,
            AddHardcoreProfileService addHardcoreProfileService
        ) : base(_routeNames, logger, config, jsonUtil)
        {
            _addHardcoreProfileService = addHardcoreProfileService;
        }

        public override bool ShouldCreateRoutes() => true;

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            bool isUsingHardcoreProfile = _addHardcoreProfileService.IsUsingHardcoreProfile();
            return JsonUtil.SerializeToValueTask(isUsingHardcoreProfile);
        }
    }
}
