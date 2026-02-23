using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class WhitelistWithInspectionState
    {
        [DataMember(Name ="inspected")]
        public string[] Inspected { get; set; } = new string[0];

        [DataMember(Name ="uninspected")]
        public string[] Uninspected { get; set; } = new string[0];

        public WhitelistWithInspectionState()
        {

        }
    }
}
