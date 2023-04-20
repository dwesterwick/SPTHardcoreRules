using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using EFT;
using EFT.UI.Matchmaker;
using SPTHardcoreRules.Models;

namespace SPTHardcoreRules.Patches
{
    public class UpdateSideSelectionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchMakerSideSelectionScreen.GClass2769).GetMethod("UpdateSideSelection", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(ESideType side)
        {
            CurrentRaidSettings.SelectedSide = side;
        }
    }
}
