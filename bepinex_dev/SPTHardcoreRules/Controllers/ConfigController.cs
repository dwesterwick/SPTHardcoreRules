using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPT.Common.Http;
using UnityEngine;
using Newtonsoft.Json;

namespace SPTHardcoreRules.Controllers
{
    public class ConfigController : MonoBehaviour
    {
        public static Configuration.ModConfig Config { get; private set; } = null;
        public static bool UsingHardcoreProfile { get; private set; } = false;

        public static Configuration.ModConfig GetConfig()
        {
            string json = RequestHandler.GetJson("/SPTHardcoreRules/GetConfig");
            Configuration.ConfigResponse response = JsonConvert.DeserializeObject<Configuration.ConfigResponse>(json);
            Config = response.Config;
            UsingHardcoreProfile = response.UsingHardcoreProfile;

            return Config;
        }
    }
}
