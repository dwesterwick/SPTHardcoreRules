using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    public class ConfigResponse
    {
        [DataMember(Name ="config")]
        public ModConfig Config { get; set; } = new ModConfig();

        [DataMember(Name ="usingHardcoreProfile")]
        public bool UsingHardcoreProfile { get; set; }

        public ConfigResponse()
        {

        }
    }
}
