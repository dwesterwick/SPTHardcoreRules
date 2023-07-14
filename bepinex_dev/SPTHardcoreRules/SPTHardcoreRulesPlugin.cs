using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using EFT;
using EFT.UI;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "1.1.9.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");
            LoggingController.Logger = Logger;

            LoggingController.Logger.LogDebug("Loading SPTHardcoreRulesPlugin...getting configuration data...");
            ConfigController.GetConfig();

            if (ConfigController.Config.Enabled)
            {
                LoggingController.Logger.LogDebug("Loading SPTHardcoreRulesPlugin...enabling patches...");
                new Patches.InsuranceScreenPatch().Enable();
                new Patches.GameStartedPatch().Enable();
                new Patches.GameWorldOnDestroyPatch().Enable();
                new Patches.ItemCheckActionPatch().Enable();
                new Patches.UpdateSideSelectionPatch().Enable();
                new Patches.ShowScreenPatch().Enable();
                new Patches.GetPrioritizedGridsForLootPatch().Enable();
            }

            LoggingController.Logger.LogDebug("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
