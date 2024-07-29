using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules.Patches
{
    public class InsuranceScreenPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod("method_74", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool PatchPrefix(MainMenuController __instance, RaidSettings ___raidSettings_0)
        {
            LoggingController.Logger.LogDebug("Disabling insurance screen...");
            // The insurance screen is disabled in live Tarkov for offline raids
            ___raidSettings_0.RaidMode = ERaidMode.Local;

            // The rest of the code was copied from the original method (except for invoking other private methods in MainMenuController)
            if (___raidSettings_0.SelectedLocation.Id == "laboratory")
            {
                ___raidSettings_0.WavesSettings.IsBosses = true;
            }
            if (___raidSettings_0.RaidMode == ERaidMode.Online)
            {
                __instance.method_43();
                return false;
            }

            __instance.method_44();
            return false;
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance, RaidSettings ___raidSettings_0)
        {
            // TO DO: Is this really true?
            // This is done in SPT.SinglePlayer.Patches.MainMenu.InsuranceScreenPatch and therefore also needs to be implemented here
            ___raidSettings_0.RaidMode = ERaidMode.Local;
        }
    }
}
