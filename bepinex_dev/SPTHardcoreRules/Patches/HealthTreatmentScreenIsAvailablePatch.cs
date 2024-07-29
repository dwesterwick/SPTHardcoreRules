using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI.SessionEnd;
using SPT.Reflection.Patching;

namespace SPTHardcoreRules.Patches
{
    public class HealthTreatmentScreenIsAvailablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthTreatmentScreen).GetMethod("IsAvailable", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
