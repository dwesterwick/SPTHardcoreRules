using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT.UI;
using SPTHardcoreRules.Models;

namespace SPTHardcoreRules.Patches
{
    public class ShowScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod("ShowScreen", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(EMenuType screen)
        {
            if (screen == EMenuType.Player)
            {
                // Need to do this in case the user chooses to be a Scav in the side-selection screen but then returns to the main menu
                CurrentRaidSettings.SelectedSide = EFT.ESideType.Pmc;
            }
        }
    }
}
