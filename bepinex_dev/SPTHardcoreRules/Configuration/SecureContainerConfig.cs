using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class SecureContainerConfig
    {
        [JsonProperty("only_use_whitelists_in_this_mod")]
        public bool UseModWhitelists { get; set; }

        [JsonProperty("ignored_secure_containers")]
        public string[] IgnoredSecureContainers { get; set; } = new string[0];

        [JsonProperty("whitelist")]
        public WhitelistCollection Whitelists { get; set; } = new WhitelistCollection();

        public SecureContainerConfig()
        {

        }
    }
}
