using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT.UI;
using HardcoreRules.Models;

namespace HardcoreRules.Patches
{
    internal class ShowScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod(nameof(MainMenuControllerClass.ShowScreen), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(EMenuType screen)
        {
            if (screen == EMenuType.Player)
            {
                // Need to do this in case the user chooses to be a Scav in the side-selection screen but then returns to the main menu
                CurrentRaidSettings.SelectedSide = EFT.ESideType.Pmc;
            }
        }
    }
}
