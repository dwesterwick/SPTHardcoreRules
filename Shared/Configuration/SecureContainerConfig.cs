using System.Runtime.Serialization;

namespace HardcoreRules.Configuration
{
    public class SecureContainerConfig
    {
        [DataMember(Name ="only_use_whitelists_in_this_mod")]
        public bool UseModWhitelists { get; set; }

        [DataMember(Name ="ignored_secure_containers")]
        public string[] IgnoredSecureContainers { get; set; } = new string[0];

        [DataMember(Name ="whitelist")]
        public WhitelistCollection Whitelists { get; set; } = new WhitelistCollection();

        public SecureContainerConfig()
        {

        }
    }
}
