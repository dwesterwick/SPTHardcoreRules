using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace SPTHardcoreRules.Controllers
{
    public static class LoggingController
    {
        public const string ServerLogMessageSource = "Hardcore Rules";
        public static ManualLogSource Logger { get; set; } = null;

        public static void LogInfo(string message)
        {
            if (!ConfigController.Config.Debug.Enabled)
            {
                return;
            }

            Logger.LogInfo(message);
        }

        public static void LogWarning(string message, bool onlyForDebug = false)
        {
            if (onlyForDebug && !ConfigController.Config.Debug.Enabled)
            {
                return;
            }

            Logger.LogWarning(message);
        }

        public static void LogError(string message, bool onlyForDebug = false)
        {
            if (onlyForDebug && !ConfigController.Config.Debug.Enabled)
            {
                return;
            }

            Logger.LogError(message);
        }
    }
}
