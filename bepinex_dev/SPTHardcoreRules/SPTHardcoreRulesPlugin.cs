using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "2.0.0.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");
            LoggingController.Logger = Logger;

            LoggingController.Logger.LogInfo("Loading SPTHardcoreRulesPlugin...getting configuration data...");
            ConfigController.GetConfig();

            if (ConfigController.UsingHardcoreProfile)
            {
                if (ConfigController.Config.Enabled)
                {
                    LoggingController.Logger.LogInfo("Loading SPTHardcoreRulesPlugin...enabling patches...");
                    new Patches.InsuranceScreenPatch().Enable();
                    new Patches.GameStartedPatch().Enable();
                    new Patches.GameWorldOnDestroyPatch().Enable();
                    new Patches.ItemCheckActionPatch().Enable();
                    new Patches.UpdateSideSelectionPatch().Enable();
                    new Patches.ShowScreenPatch().Enable();
                    new Patches.GetPrioritizedContainersForLootPatch().Enable();

                    if (ConfigController.Config.Services.DisableScavRaids)
                    {
                        new Patches.SideSelectionUpdatePatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisableInsurance)
                    {
                        new Patches.DisableInsuranceForItemPatch().Enable();
                        new Patches.DisableInsuranceForItemClassPatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisableRepairs)
                    {
                        new Patches.RemoveRepairOptionPatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisablePostRaidHealing)
                    {
                        new Patches.HealthTreatmentScreenIsAvailablePatch().Enable();
                    }
                }
                else
                {
                    LoggingController.Logger.LogWarning("Loading SPTHardcoreRulesPlugin...using a hardcore profile but mod is disabled");
                }
            }
            else
            {
                LoggingController.Logger.LogWarning("Loading SPTHardcoreRulesPlugin...not using a hardcore profile");
            }

            LoggingController.Logger.LogInfo("Loading SPTHardcoreRulesPlugin...done.");
        }
    }
}
