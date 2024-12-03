using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using SPTHardcoreRules.Controllers;
using SPTHardcoreRules.Helpers;

namespace SPTHardcoreRules.Patches
{
    public class GetPrioritizedContainersForLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            string methodName = "GetPrioritizedContainersForLoot";

            Type targetType = FindTargetType(methodName);
            LoggingController.LogInfo("Found type for GetPrioritizedContainersForLootPatch: " + targetType.FullName);

            return targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ref IEnumerable<EFT.InventoryLogic.IContainer> __result, Item item)
        {
            __result = __result.Where(container => item.IsAllowedToBePlacedIn(container));
        }

        public static Type FindTargetType(string methodName)
        {
            List<Type> targetTypeOptions = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => t.GetMethods().Any(m => m.Name.Contains(methodName)))
                .ToList();

            if (targetTypeOptions.Count != 1)
            {
                throw new TypeLoadException("Cannot find any type containing method " + methodName);
            }

            return targetTypeOptions[0];
        }
    }
}
