using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Configuration
{
    public class WhitelistWithInspectionState
    {
        [JsonProperty("inspected")]
        public Whitelist Inspected { get; set; } = new Whitelist();

        [JsonProperty("uninspected")]
        public Whitelist Uninspected { get; set; } = new Whitelist();

        public WhitelistWithInspectionState()
        {

        }
    }
}
