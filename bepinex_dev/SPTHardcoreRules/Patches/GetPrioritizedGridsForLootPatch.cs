using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules.Patches
{
    public class GetPrioritizedGridsForLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            string methodName = "GetPrioritizedGridsForLoot";

            Type targetType = FindTargetType(methodName);
            LoggingController.LogInfo("Found type for GetPrioritizedGridsForLootPatch: " + targetType.FullName);

            return targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref IEnumerable<StashGridClass> __result, Item item)
        {
            // Remove containers in which the item is blacklisted
            List<string> blacklistedContainerTemplateIDs = new List<string>();
            foreach (StashGridClass container in __result)
            {
                bool canUse = ItemCheckActionPatch.PatchPostfix(true, item, container.ParentItem.CurrentAddress);
                if (!canUse)
                {
                    blacklistedContainerTemplateIDs.Add(container.ParentItem.TemplateId);
                }
            }

            __result = __result.Where(i => !blacklistedContainerTemplateIDs.Contains(i.ParentItem.TemplateId));
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
