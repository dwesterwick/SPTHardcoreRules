using BepInEx;
using Comfort.Common;
using HardcoreRules.Helpers;
using HardcoreRules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules
{
    [BepInPlugin(ModInfo.GUID, ModInfo.MODNAME, ModInfo.MOD_VERSION)]
    internal class HardcoreRulesPugin : BaseUnityPlugin
    {
        protected void Awake()
        {
            Logger.LogInfo("Loading HardcoreRulesPlugin...");

            Singleton<LoggingUtil>.Create(new LoggingUtil(Logger));

            if (ConfigUtil.UsingHardcoreProfile)
            {
                enablePatches();
            }
            else
            {
                Singleton<LoggingUtil>.Instance.LogWarning("Loading HardcoreRulesPlugin...not using a hardcore profile");
            }

            Singleton<LoggingUtil>.Instance.LogInfo("Loading HardcoreRulesPlugin...done.");
        }

        private void enablePatches()
        {
            new Patches.MenuShowPatch().Enable();

            if (!ConfigUtil.CurrentConfig.IsModEnabled())
            {
                Singleton<LoggingUtil>.Instance.LogWarning("Loading HardcoreRulesPlugin...using a hardcore profile but mod is disabled");
                return;
            }

            Singleton<LoggingUtil>.Instance.LogInfo("Loading HardcoreRulesPlugin...enabling patches...");

            new Patches.GameStartedPatch().Enable();
            new Patches.GameWorldOnDestroyPatch().Enable();
            new Patches.UpdateSideSelectionPatch().Enable();
            new Patches.ShowScreenPatch().Enable();

            if (ConfigUtil.CurrentConfig.Services.DisableScavRaids)
            {
                new Patches.SideSelectionUpdatePatch().Enable();
            }

            if (ConfigUtil.CurrentConfig.Services.DisableInsurance)
            {
                new Patches.InsuranceScreenPatch().Enable();
                new Patches.DisableInsuranceForItemPatch().Enable();
                new Patches.DisableInsuranceForItemClassPatch().Enable();
            }

            if (ConfigUtil.CurrentConfig.Services.DisableTraderRepairs)
            {
                new Patches.RemoveRepairOptionPatch().Enable();
            }

            if (ConfigUtil.CurrentConfig.Services.DisablePostRaidHealing)
            {
                new Patches.HealthTreatmentScreenShowPatch().Enable();
                new Patches.HealthTreatmentScreenAddTreatmentPatch().Enable();
            }

            if (ConfigUtil.CurrentConfig.SecureContainer.UseModWhitelists)
            {
                new Patches.ItemCheckActionPatch().Enable();
                new Patches.GetPrioritizedContainersForLootPatch().Enable();
            }
        }
    }
}
