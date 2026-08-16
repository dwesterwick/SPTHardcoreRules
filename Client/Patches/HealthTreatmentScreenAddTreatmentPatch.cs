using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using SPT.Reflection.Patching;

namespace HardcoreRules.Patches
{
    internal class HealthTreatmentScreenAddTreatmentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthTreatmentServiceView).GetMethod(nameof(HealthTreatmentServiceView.AddTreatment), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        protected static bool PatchPrefix(HealthTreatmentServiceView __instance, ref bool ____nothingToHeal)
        {
            __instance.RecalculateCost();
            ____nothingToHeal = false;

            return false;
        }
    }
}
