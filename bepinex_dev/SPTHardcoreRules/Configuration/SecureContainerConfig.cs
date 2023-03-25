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
        [JsonProperty("more_restrictions")]
        public bool MoreRestrictions { get; set; }

        [JsonProperty("restrict_whitelisted_items")]
        public bool RestrictWhitelistedItems { get; set; }

        [JsonProperty("ignored_secure_containers")]
        public string[] IgnoredSecureContainers { get; set; } = new string[0];

        [JsonProperty("whitelist")]
        public WhitelistCollection Whitelists { get; set; } = new WhitelistCollection();

        public SecureContainerConfig()
        {

        }
    }
}
