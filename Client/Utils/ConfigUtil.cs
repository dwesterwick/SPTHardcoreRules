using HardcoreRules.Helpers;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Utils
{
    public static class ConfigUtil
    {
        private static bool _usingHardcoreProfile = false;
        public static bool UsingHardcoreProfile
        {
            get
            {
                if (_currentConfig == null)
                {
                    GetConfig();
                }

                return _usingHardcoreProfile;
            }
        }

        private static Configuration.ModConfig? _currentConfig;
        public static Configuration.ModConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    GetConfig();
                }

                return _currentConfig!;
            }
        }

        private static void GetConfig()
        {
            string routeName = SharedRouterHelpers.GetRoutePath("GetConfig");

            string json = RequestHandler.GetJson(routeName);
            Configuration.ConfigResponse? configResponse = JsonConvert.DeserializeObject<Configuration.ConfigResponse>(json);
            if (configResponse == null)
            {
                throw new InvalidOperationException("Could not deserialize config file");
            }

            _usingHardcoreProfile = configResponse.UsingHardcoreProfile;
            _currentConfig = configResponse.Config;
        }
    }
}
