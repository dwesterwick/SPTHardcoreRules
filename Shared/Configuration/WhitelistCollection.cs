using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class WhitelistCollection
    {
        [DataMember(Name ="inRaid")]
        public WhitelistWithInspectionState InRaid { get; set; } = new WhitelistWithInspectionState();

        [DataMember(Name ="inHideout")]
        public Whitelist InHideout { get; set; } = new Whitelist();

        [DataMember(Name ="global")]
        public Whitelist Global { get; set; } = new Whitelist();

        public WhitelistCollection()
        {

        }
    }
}
