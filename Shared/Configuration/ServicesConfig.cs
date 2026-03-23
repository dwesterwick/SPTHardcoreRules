using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class ServicesConfig
    {
        [DataMember(Name = "flea_market", IsRequired = true)]
        public FleaMarketConfig FleaMarket { get; set; } = new FleaMarketConfig();

        [DataMember(Name = "disable_trader_repairs", IsRequired = true)]
        public bool DisableTraderRepairs { get; set; }

        [DataMember(Name = "disable_insurance", IsRequired = true)]
        public bool DisableInsurance { get; set; }

        [DataMember(Name = "disable_post_raid_healing", IsRequired = true)]
        public bool DisablePostRaidHealing { get; set; }

        [DataMember(Name = "disable_scav_raids", IsRequired = true)]
        public bool DisableScavRaids { get; set; }

        public ServicesConfig()
        {

        }
    }
}
