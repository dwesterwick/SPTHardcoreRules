using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using EFT;
using SPTHardcoreRules.Configuration;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "1.1.0.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        public static Configuration.ModConfig ModConfig { get; set; } = null;
        public static ISession CurrentSession { get; set; } = null;

        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");
            new Patches.ShowScreenPatch().Enable();
            new Patches.InsuranceScreenPatch().Enable();

            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...getting configuration data...");
            ModConfig = Controllers.ConfigController.GetConfig();

            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
