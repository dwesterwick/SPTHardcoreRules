using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using EFT.UI.Matchmaker;
using SPT.Reflection.Patching;

namespace SPTHardcoreRules.Patches
{
    public class SideSelectionAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchMakerSideSelectionScreen).GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(UIAnimatedToggleSpawner ____savagesButton)
        {
            ____savagesButton.GameObject.SetActive(false);
        }
    }
}
