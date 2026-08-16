using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using EFT.UI.Matchmaker;
using HardcoreRules.Models;

namespace HardcoreRules.Patches
{
    internal class UpdateSideSelectionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchMakerSideSelectionScreen.RaidSideSelectionScreenController)
                .GetMethod(nameof(MatchMakerSideSelectionScreen.RaidSideSelectionScreenController.UpdateSideSelection), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ESideType side)
        {
            CurrentRaidSettings.SelectedSide = side;
        }
    }
}
