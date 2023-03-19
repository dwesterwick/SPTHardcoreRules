using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using Aki.Reflection.Utils;
using HarmonyLib;
using SPTHardcoreRules;

namespace TestClientMod
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
            if (SPTHardcoreRulesPlugin.InsuranceEnabled)
            {
                Logger.LogInfo("Allowing insurance screen...");
                ___raidSettings_0.RaidMode = ERaidMode.Online;
            }
            else
            {
                Logger.LogInfo("Disabling insurance screen...");
                ___raidSettings_0.RaidMode = ERaidMode.Local;
            }

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
            ___raidSettings_0.RaidMode = ERaidMode.Local;
        }
    }
}
