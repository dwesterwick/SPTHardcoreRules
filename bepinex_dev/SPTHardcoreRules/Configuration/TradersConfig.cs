using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class TradersConfig
    {
        [JsonProperty("ID_money")]
        public string ID_money { get; set; }

        [JsonProperty("disable_fence")]
        public bool DisableFence { get; set; }

        [JsonProperty("barters_only")]
        public bool BartersOnly { get; set; }

        [JsonProperty("whitelist_only")]
        public bool WhitelistOnly { get; set; }

        [JsonProperty("whitelist")]
        public Whitelist Whitelist { get; set; } = new Whitelist();

        public TradersConfig()
        {

        }
    }
}
