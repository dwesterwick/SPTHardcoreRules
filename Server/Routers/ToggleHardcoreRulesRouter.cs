using HardcoreRules.Helpers;
using HardcoreRules.Routers.Internal;
using HardcoreRules.Services;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    public class ToggleHardcoreRulesRouter : AbstractStaticRouter
    {
        private static readonly string[] _routeNames = ["ToggleHardcoreRules"];

        private ProfileUtil _profileUtil = null!;
        private ToggleHardcoreRulesService _toggleHardcoreRulesService = null!;

        public ToggleHardcoreRulesRouter
        (
            LoggingUtil logger,
            ConfigUtil config,
            JsonUtil jsonUtil,
            ProfileUtil profileUtil,
            ToggleHardcoreRulesService toggleHardcoreRulesService
        ) : base(_routeNames, logger, config, jsonUtil)
        {
            _profileUtil = profileUtil;
            _toggleHardcoreRulesService ??= toggleHardcoreRulesService;
        }

        public override bool ShouldCreateRoutes() => true;

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            ToggleHardcoreRules(routerData.SessionId);

            return JsonUtil.SerializeToValueTask(_toggleHardcoreRulesService.HardcoreRulesEnabled);
        }

        private void ToggleHardcoreRules(MongoId sessionId)
        {
            bool useHardcoreRules = IsUsingAHardcoreProfile(sessionId) || Config.CurrentConfig.UseForAllProfiles;

            _toggleHardcoreRulesService.ToggleHardcoreRules(useHardcoreRules);
            Logger.Info($"Hardcore rules were {(useHardcoreRules ? "" : "not")} enabled.");
        }

        private bool IsUsingAHardcoreProfile(MongoId sessionId)
        {
            string? username = _profileUtil.GetUsername(sessionId);
            bool isUsingAHardcoreProfile = _profileUtil.IsUsingAHardcoreProfile(sessionId);

            Logger.Info($"{username} is {(isUsingAHardcoreProfile ? "" : "not")} using a hardcore profile.");
            return isUsingAHardcoreProfile;
        }
    }
}
