using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class DebugConfig
    {
        [DataMember(Name = "enabled", IsRequired = true)]
        public bool Enabled { get; set; }

        [DataMember(Name = "flea_market_min_level", IsRequired = true)]
        public int FleaMarketMinLevel { get; set; }

        public DebugConfig()
        {

        }
    }
}
