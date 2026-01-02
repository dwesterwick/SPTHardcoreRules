using HardcoreRules.Configuration;
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
        private static bool? _usingHardcoreProfile = null!;
        public static bool UsingHardcoreProfile
        {
            get
            {
                if (_usingHardcoreProfile == null)
                {
                    _usingHardcoreProfile = IsUsingHardcoreProfile();
                }

                return _usingHardcoreProfile.Value;
            }
        }

        private static Configuration.ModConfig? _currentConfig;
        public static Configuration.ModConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    _currentConfig = GetConfig();
                }

                return _currentConfig!;
            }
        }

        private static ModConfig GetConfig()
        {
            string routeName = SharedRouterHelpers.GetRoutePath("GetConfig");

            string json = RequestHandler.GetJson(routeName);
            ModConfig? response = JsonConvert.DeserializeObject<ModConfig>(json);
            if (response == null)
            {
                throw new InvalidOperationException("Could not retrieve mod configuration from the server");
            }

            return response;
        }

        private static bool IsUsingHardcoreProfile()
        {
            string routeName = SharedRouterHelpers.GetRoutePath("ToggleHardcoreRules");

            string json = RequestHandler.GetJson(routeName);
            bool? response = JsonConvert.DeserializeObject<bool>(json);
            if (response == null)
            {
                throw new InvalidOperationException("Could not receive response from server to determine if a hardcore profile is being used");
            }

            return response.Value;
        }
    }
}
