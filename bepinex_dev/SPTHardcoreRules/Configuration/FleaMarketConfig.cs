using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class FleaMarketConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("only_barter_offers")]
        public bool OnlyBarterOffers { get; set; }

        [JsonProperty("min_level")]
        public int MinLevel { get; set; }

        public FleaMarketConfig()
        {

        }
    }
}
