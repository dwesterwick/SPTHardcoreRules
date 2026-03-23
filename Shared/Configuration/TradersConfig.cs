using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class TradersConfig
    {
        [DataMember(Name = "disable_fence", IsRequired = true)]
        public bool DisableFence { get; set; }

        [DataMember(Name = "disable_starting_gifts", IsRequired = true)]
        public bool DisableStartingGifts { get; set; }

        [DataMember(Name = "barters_only", IsRequired = true)]
        public bool BartersOnly { get; set; }

        [DataMember(Name = "allow_GPCoins", IsRequired = true)]
        public bool AllowGPCoins { get; set; }

        [DataMember(Name = "whitelist_only", IsRequired = true)]
        public bool WhitelistOnly { get; set; }

        [DataMember(Name = "whitelist_items", IsRequired = true)]
        public string[] WhitelistItems { get; set; } = new string[0];

        [DataMember(Name = "whitelist_traders", IsRequired = true)]
        public string[] WhitelistTraders { get; set; } = new string[0];

        public TradersConfig()
        {

        }
    }
}
