using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class FleaMarketConfig
    {
        [DataMember(Name ="enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name ="only_barter_offers")]
        public bool OnlyBarterOffers { get; set; }

        public FleaMarketConfig()
        {

        }
    }
}
