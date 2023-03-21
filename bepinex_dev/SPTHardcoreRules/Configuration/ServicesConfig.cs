using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class ServicesConfig
    {
        [JsonProperty("flea_market")]
        public FleaMarketConfig FleaMarket { get; set; } = new FleaMarketConfig();

        [JsonProperty("disable_repairs")]
        public bool DisableRepairs { get; set; }

        [JsonProperty("disable_insurance")]
        public bool DisableInsurance { get; set; }

        [JsonProperty("disable_post_raid_healing")]
        public bool DisablePostRaidHealing { get; set; }

        [JsonProperty("disable_scav_raids")]
        public bool DisableScavRaids { get; set; }

        public ServicesConfig()
        {

        }
    }
}
