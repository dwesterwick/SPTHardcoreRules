using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aki.Common.Http;
using UnityEngine;
using Newtonsoft.Json;

namespace SPTHardcoreRules.Controllers
{
    public class ConfigController : MonoBehaviour
    {
        public static Configuration.ModConfig Config { get; set; } = null;

        public static Configuration.ModConfig GetConfig()
        {
            string json = RequestHandler.GetJson("/SPTHardcoreRules/GetConfig");
            Config = JsonConvert.DeserializeObject<Configuration.ModConfig>(json);
            return Config;
        }
    }
}
