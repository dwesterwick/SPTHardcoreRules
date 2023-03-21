using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class WhitelistCollection
    {
        [JsonProperty("inRaid")]
        public WhitelistWithInspectionState InRaid { get; set; } = new WhitelistWithInspectionState();

        [JsonProperty("inHideout")]
        public Whitelist InHideout { get; set; } = new Whitelist();

        [JsonProperty("global")]
        public Whitelist Global { get; set; } = new Whitelist();

        public WhitelistCollection()
        {

        }
    }
}
