using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using EFT;

namespace SPTHardcoreRules.Patches
{
    public class InsuranceScreenPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod("method_66", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool PatchPrefix(MainMenuController __instance, RaidSettings ___raidSettings_0)
        {
            if (SPTHardcoreRulesPlugin.ModConfig.Services.DisableInsurance)
            {
                Logger.LogDebug("Disabling insurance screen...");
                // The insurance screen is disabled in live Tarkov for offline raids
                ___raidSettings_0.RaidMode = ERaidMode.Local;
            }
            else
            {
                Logger.LogDebug("Allowing insurance screen...");
                // This is done in Aki.SinglePlayer.Patches.MainMenu.InsuranceScreenPatch and therefore also needs to be implemented here
                ___raidSettings_0.RaidMode = ERaidMode.Online;
            }

            // The rest of the code was copied from the original method (except for invoking other private methods in MainMenuController)
            if (___raidSettings_0.SelectedLocation.Id == "laboratory")
            {
                ___raidSettings_0.WavesSettings.IsBosses = true;
            }
            if (___raidSettings_0.RaidMode == ERaidMode.Online)
            {
                MethodInfo method_38 = typeof(MainMenuController).GetMethod("method_38", BindingFlags.NonPublic | BindingFlags.Instance);
                method_38.Invoke(__instance, new object[] { });

                return false;
            }

            MethodInfo method_39 = typeof(MainMenuController).GetMethod("method_39", BindingFlags.NonPublic | BindingFlags.Instance);
            method_39.Invoke(__instance, new object[] { } );

            return false;
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance, RaidSettings ___raidSettings_0)
        {
            // This is done in Aki.SinglePlayer.Patches.MainMenu.InsuranceScreenPatch and therefore also needs to be implemented here
            ___raidSettings_0.RaidMode = ERaidMode.Local;
        }
    }
}
