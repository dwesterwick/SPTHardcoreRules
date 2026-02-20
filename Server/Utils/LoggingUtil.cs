using HardcoreRules.Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace HardcoreRules.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class LoggingUtil(ISptLogger<HardcoreRules_Server> _logger, ConfigUtil _configUtil)
    {
        public void Debug(string message)
        {
            if (!_configUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.Debug(GetLogPrefix() + message);
        }

        public void Info(string message)
        {
            if (!_configUtil.CurrentConfig.IsDebugEnabled())
            {
                return;
            }

            _logger.Info(GetLogPrefix() + message);
        }

        public void Warning(string message)
        {
            _logger.Warning(GetLogPrefix() + message);
        }

        public void Error(string message)
        {
            _logger.Error(GetLogPrefix() + message);
        }

        private string GetLogPrefix()
        {
            return $"[{ModInfo.MODNAME}] ";
        }
    }
}