using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    public class Whitelist
    {
        [DataMember(Name ="parents")]
        public string[] ID_Parents { get; set; } = new string[0];

        [DataMember(Name ="items")]
        public string[] ID_Items { get; set; } = new string[0];

        public Whitelist()
        {

        }
    }
}
