using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class ModConfig
    {
        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; } = false;

        [DataMember(Name = "use_for_all_profiles")]
        public bool UseForAllProfiles { get; set; }

        [DataMember(Name = "debug")]
        public DebugConfig Debug { get; set; } = new DebugConfig();

        [DataMember(Name = "services")]
        public ServicesConfig Services { get; set; } = new ServicesConfig();

        [DataMember(Name = "traders")]
        public TradersConfig Traders { get; set; } = new TradersConfig();

        [DataMember(Name = "secureContainer")]
        public SecureContainerConfig SecureContainer { get; set; } = new SecureContainerConfig();
    }
}
