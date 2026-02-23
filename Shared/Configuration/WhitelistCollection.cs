using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class WhitelistCollection
    {
        [DataMember(Name ="inRaid")]
        public WhitelistWithInspectionState InRaid { get; set; } = new WhitelistWithInspectionState();

        [DataMember(Name ="inHideout")]
        public string[] InHideout { get; set; } = new string[0];

        [DataMember(Name ="global")]
        public string[] Global { get; set; } = new string[0];

        public WhitelistCollection()
        {

        }
    }
}
