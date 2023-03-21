using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI;
using SPTHardcoreRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Patches
{
    public class ShowScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod("ShowScreen", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance, EMenuType screen, bool turnOn, ISession ___ginterface128_0)
        {
            Logger.LogInfo("ShowScreen/" + screen.ToString());

            SPTHardcoreRulesPlugin.CurrentSession = ___ginterface128_0;

            if ((SPTHardcoreRulesPlugin.CurrentSession == null) || (SPTHardcoreRulesPlugin.CurrentSession.Profile == null))
                return;

            Logger.LogInfo("SessionSide/" + SPTHardcoreRulesPlugin.CurrentSession.Profile.Side.ToString());
        }
    }
}
