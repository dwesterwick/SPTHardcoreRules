using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using HardcoreRules.Utils;
using Comfort.Common;

namespace HardcoreRules.Patches
{
    public class InsuranceScreenPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod("method_80", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        protected static bool PatchPrefix(MainMenuControllerClass __instance, RaidSettings ___RaidSettings_0, RaidSettings ___RaidSettings_1)
        {
            Singleton<LoggingUtil>.Instance.LogDebug("Disabling insurance screen...");
            // The insurance screen is disabled in live Tarkov for offline raids
            ___RaidSettings_0.RaidMode = ERaidMode.Local;

            // The rest of the code was copied from the original method (except for invoking other private methods in MainMenuController)
            if (___RaidSettings_0.SelectedLocation.Id == "laboratory")
            {
                ___RaidSettings_0.WavesSettings.IsBosses = true;
                ___RaidSettings_1.WavesSettings.IsBosses = true;
            }
            if (___RaidSettings_0.RaidMode == ERaidMode.Online)
            {
                __instance.method_51();
                return false;
            }

            __instance.method_52();
            return false;
        }

        [PatchPostfix]
        protected static void PatchPostfix(MainMenuControllerClass __instance, RaidSettings ___RaidSettings_0)
        {
            // TO DO: Is this really true?
            // This is done in SPT.SinglePlayer.Patches.MainMenu.InsuranceScreenPatch and therefore also needs to be implemented here
            ___RaidSettings_0.RaidMode = ERaidMode.Local;
        }
    }
}
