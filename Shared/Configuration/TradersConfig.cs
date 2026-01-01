using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    public class TradersConfig
    {
        [DataMember(Name = "ID_money")]
        public string ID_money { get; set; } = null!;

        [DataMember(Name ="ID_GPCoins")]
        public string ID_GPCoins { get; set; } = null!;

        [DataMember(Name ="disable_fence")]
        public bool DisableFence { get; set; }

        [DataMember(Name ="disable_starting_gifts")]
        public bool DisableStartingGifts { get; set; }

        [DataMember(Name ="barters_only")]
        public bool BartersOnly { get; set; }

        [DataMember(Name ="allow_GPCoins")]
        public bool AllowGPCoins { get; set; }

        [DataMember(Name ="whitelist_only")]
        public bool WhitelistOnly { get; set; }

        [DataMember(Name ="whitelist_items")]
        public Whitelist WhitelistItems { get; set; } = new Whitelist();

        [DataMember(Name ="whitelist_traders")]
        public string[] WhitelistTraders { get; set; } = new string[0];

        public TradersConfig()
        {

        }
    }
}
