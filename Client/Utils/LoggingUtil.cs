using HardcoreRules.Helpers;

namespace HardcoreRules.Utils
{
    internal class LoggingUtil
    {
        private BepInEx.Logging.ManualLogSource _logger;

        public LoggingUtil(BepInEx.Logging.ManualLogSource logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message)
        {
            if (!ConfigUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.LogDebug(message);
        }

        public void LogInfo(string message)
        {
            if (!ConfigUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.LogInfo(message);
        }

        public void LogWarning(string message, bool onlyForDebug = false)
        {
            if (onlyForDebug && !ConfigUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.LogWarning(message);
        }

        public void LogError(string message, bool onlyForDebug = false)
        {
            if (onlyForDebug && !ConfigUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.LogError(message);
        }
    }
}