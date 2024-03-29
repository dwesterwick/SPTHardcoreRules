﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SPTHardcoreRules.Configuration
{
    public class ModConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("services")]
        public ServicesConfig Services { get; set; } = new ServicesConfig();

        [JsonProperty("traders")]
        public TradersConfig Traders { get; set; } = new TradersConfig();

        [JsonProperty("secureContainer")]
        public SecureContainerConfig SecureContainer { get; set; } = new SecureContainerConfig();

        public ModConfig()
        {

        }
    }
}
