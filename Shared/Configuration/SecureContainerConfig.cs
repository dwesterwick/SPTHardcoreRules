using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    [DataContract]
    public class SecureContainerConfig
    {
        [DataMember(Name = "only_use_whitelists_in_this_mod", IsRequired = true)]
        public bool UseModWhitelists { get; set; }

        [DataMember(Name = "ignored_secure_containers", IsRequired = true)]
        public string[] IgnoredSecureContainers { get; set; } = new string[0];

        [DataMember(Name = "whitelist", IsRequired = true)]
        public WhitelistCollection Whitelists { get; set; } = new WhitelistCollection();

        public SecureContainerConfig()
        {

        }
    }
}
