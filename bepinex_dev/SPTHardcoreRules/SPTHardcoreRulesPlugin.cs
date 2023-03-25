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
        public static bool IsInRaid { get; set; } = false;

        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");

            Logger.LogDebug("Loading SPTHardcoreRulesPlugin...getting configuration data...");
            ModConfig = Controllers.ConfigController.GetConfig();

            Logger.LogDebug("Loading SPTHardcoreRulesPlugin...enabling patches...");
            new Patches.InsuranceScreenPatch().Enable();
            new Patches.GameStartedPatch().Enable();
            new Patches.GameWorldOnDestroyPatch().Enable();
            new Patches.ItemCheckActionPatch().Enable();

            Logger.LogDebug("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
