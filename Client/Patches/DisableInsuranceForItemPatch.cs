using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace HardcoreRules.Patches
{
    internal class DisableInsuranceForItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InsuranceCompanyClass).GetMethod(
                nameof(InsuranceCompanyClass.ItemTypeAvailableForInsurance),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof(Item) },
                null);
        }

        [PatchPrefix]
        protected static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
