using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class ModConfig
    {
        [DataMember(Name =  "enabled", IsRequired = true)]
        public bool Enabled { get; set; } = false;

        [DataMember(Name =  "use_for_all_profiles", IsRequired = true)]
        public bool UseForAllProfiles { get; set; }

        [DataMember(Name =  "debug", IsRequired = true)]
        public DebugConfig Debug { get; set; } = new DebugConfig();

        [DataMember(Name =  "services", IsRequired = true)]
        public ServicesConfig Services { get; set; } = new ServicesConfig();

        [DataMember(Name =  "traders", IsRequired = true)]
        public TradersConfig Traders { get; set; } = new TradersConfig();

        [DataMember(Name =  "secureContainer", IsRequired = true)]
        public SecureContainerConfig SecureContainer { get; set; } = new SecureContainerConfig();

        public ModConfig()
        {

        }
    }
}
