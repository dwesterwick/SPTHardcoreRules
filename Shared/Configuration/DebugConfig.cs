using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class DebugConfig
    {
        [DataMember(Name ="enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name ="flea_market_min_level")]
        public int FleaMarketMinLevel { get; set; }

        public DebugConfig()
        {

        }
    }
}
