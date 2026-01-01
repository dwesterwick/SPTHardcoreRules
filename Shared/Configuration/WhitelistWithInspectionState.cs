using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    public class WhitelistWithInspectionState
    {
        [DataMember(Name ="inspected")]
        public Whitelist Inspected { get; set; } = new Whitelist();

        [DataMember(Name ="uninspected")]
        public Whitelist Uninspected { get; set; } = new Whitelist();

        public WhitelistWithInspectionState()
        {

        }
    }
}
