using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class WhitelistCollection
    {
        [DataMember(Name = "inRaid", IsRequired = true)]
        public WhitelistWithInspectionState InRaid { get; set; } = new WhitelistWithInspectionState();

        [DataMember(Name = "inHideout", IsRequired = true)]
        public string[] InHideout { get; set; } = new string[0];

        [DataMember(Name = "global", IsRequired = true)]
        public string[] Global { get; set; } = new string[0];

        public WhitelistCollection()
        {

        }
    }
}
