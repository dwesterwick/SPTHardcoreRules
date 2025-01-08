using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules
{
    [BepInPlugin("com.DanW.SPTHardcoreRules", "SPTHardcoreRulesPlugin", "2.1.1.0")]
    public class SPTHardcoreRulesPlugin : BaseUnityPlugin
    {
        protected void Awake()
        {
            Logger.LogInfo("Loading SPTHardcoreRulesPlugin...");
            LoggingController.Logger = Logger;

            LoggingController.Logger.LogInfo("Loading SPTHardcoreRulesPlugin...getting configuration data...");
            ConfigController.GetConfig();

            if (ConfigController.UsingHardcoreProfile)
            {
                new Patches.MenuShowPatch().Enable();

                if (ConfigController.Config.Enabled)
                {
                    LoggingController.Logger.LogInfo("Loading SPTHardcoreRulesPlugin...enabling patches...");
                    
                    new Patches.GameStartedPatch().Enable();
                    new Patches.GameWorldOnDestroyPatch().Enable();
                    new Patches.UpdateSideSelectionPatch().Enable();
                    new Patches.ShowScreenPatch().Enable();
                    
                    if (ConfigController.Config.Services.DisableScavRaids)
                    {
                        new Patches.SideSelectionUpdatePatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisableInsurance)
                    {
                        new Patches.InsuranceScreenPatch().Enable();
                        new Patches.DisableInsuranceForItemPatch().Enable();
                        new Patches.DisableInsuranceForItemClassPatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisableTraderRepairs)
                    {
                        new Patches.RemoveRepairOptionPatch().Enable();
                    }

                    if (ConfigController.Config.Services.DisablePostRaidHealing)
                    {
                        new Patches.HealthTreatmentScreenShowPatch().Enable();
                        new Patches.HealthTreatmentScreenAddTreatmentPatch().Enable();
                    }

                    if (ConfigController.Config.SecureContainer.UseModWhitelists)
                    {
                        new Patches.ItemCheckActionPatch().Enable();
                        new Patches.GetPrioritizedContainersForLootPatch().Enable();
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
