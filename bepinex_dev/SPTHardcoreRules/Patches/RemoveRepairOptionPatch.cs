using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace SPTHardcoreRules.Patches
{
    public class RemoveRepairOptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            string methodName = "IsActive";

            //Type targetType = FindTargetType(methodName);

            return typeof(GClass3074).GetMethod("IsActive", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref bool __result, EItemInfoButton button)
        {
            if (button == EItemInfoButton.Repair)
            {
                __result = false;
            }
        }

        public static Type FindTargetType(string methodName)
        {
            List<Type> targetTypeOptions = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => t.GetProperties().Any(p => p.Name.Contains("IsOwnedByPlayer")))
                .Where(t => t
                    .GetMethods()
                    .Any(m => m.Name.Contains(methodName) && m.GetParameters().All(p => p.ParameterType == typeof(EItemInfoButton))))
                .ToList();

            if (targetTypeOptions.Count != 1)
            {
                throw new TypeLoadException("Cannot find matching type containing method " + methodName);
            }

            return targetTypeOptions[0];
        }
    }
}
