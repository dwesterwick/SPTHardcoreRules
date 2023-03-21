using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class Whitelist
    {
        [JsonProperty("parents")]
        public string[] ID_Parents { get; set; } = new string[0];

        [JsonProperty("items")]
        public string[] ID_Items { get; set; } = new string[0];

        public Whitelist()
        {

        }
    }
}
