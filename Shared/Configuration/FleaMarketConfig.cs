using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class FleaMarketConfig
    {
        [DataMember(Name = "enabled", IsRequired = true)]
        public bool Enabled { get; set; }

        [DataMember(Name = "only_barter_offers", IsRequired = true)]
        public bool OnlyBarterOffers { get; set; }

        public FleaMarketConfig()
        {

        }
    }
}
