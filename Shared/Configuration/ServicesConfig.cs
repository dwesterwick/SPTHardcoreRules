using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class ServicesConfig
    {
        [DataMember(Name ="flea_market")]
        public FleaMarketConfig FleaMarket { get; set; } = new FleaMarketConfig();

        [DataMember(Name ="disable_trader_repairs")]
        public bool DisableTraderRepairs { get; set; }

        [DataMember(Name ="disable_insurance")]
        public bool DisableInsurance { get; set; }

        [DataMember(Name ="disable_post_raid_healing")]
        public bool DisablePostRaidHealing { get; set; }

        [DataMember(Name ="disable_scav_raids")]
        public bool DisableScavRaids { get; set; }

        public ServicesConfig()
        {

        }
    }
}
