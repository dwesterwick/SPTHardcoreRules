using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class ConfigResponse
    {
        [JsonProperty("config")]
        public ModConfig Config { get; set; } = new ModConfig();

        [JsonProperty("usingHardcoreProfile")]
        public bool UsingHardcoreProfile { get; set; }

        public ConfigResponse()
        {

        }
    }
}
