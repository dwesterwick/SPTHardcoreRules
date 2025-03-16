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

        [JsonProperty("ID_GPCoins")]
        public string ID_GPCoins { get; set; }

        [JsonProperty("disable_fence")]
        public bool DisableFence { get; set; }

        [JsonProperty("disable_starting_gifts")]
        public bool DisableStartingGifts { get; set; }

        [JsonProperty("barters_only")]
        public bool BartersOnly { get; set; }

        [JsonProperty("allow_GPCoins")]
        public bool AllowGPCoins { get; set; }

        [JsonProperty("whitelist_only")]
        public bool WhitelistOnly { get; set; }

        [JsonProperty("whitelist_items")]
        public Whitelist WhitelistItems { get; set; } = new Whitelist();

        [JsonProperty("whitelist_traders")]
        public string[] WhitelistTraders { get; set; } = new string[0];

        public TradersConfig()
        {

        }
    }
}
