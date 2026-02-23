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
    internal class ToggleHardcoreRulesRouter : AbstractStaticRouter
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
            bool shouldUseHardcoreRules = ShouldUseHardcoreRules(routerData.SessionId);

            if (Config.CurrentConfig.IsModEnabled())
            {
                _toggleHardcoreRulesService.ToggleHardcoreRules(shouldUseHardcoreRules);
                Logger.Info($"Hardcore rules were {(shouldUseHardcoreRules ? "" : "not ")}enabled.");
            }
            
            return JsonUtil.SerializeToValueTask(shouldUseHardcoreRules);
        }

        private bool ShouldUseHardcoreRules(MongoId sessionId)
        {
            return IsUsingAHardcoreProfile(sessionId) || Config.CurrentConfig.UseForAllProfiles;
        }

        private bool IsUsingAHardcoreProfile(MongoId sessionId)
        {
            string? username = _profileUtil.GetUsername(sessionId);
            bool isUsingAHardcoreProfile = _profileUtil.IsUsingAHardcoreProfile(sessionId);

            Logger.Info($"{username} is {(isUsingAHardcoreProfile ? "" : "not ")}using a hardcore profile.");
            return isUsingAHardcoreProfile;
        }
    }
}
