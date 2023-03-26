using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Patches
{
    public class GetPrioritizedGridsForLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass2399).GetMethod("GetPrioritizedGridsForLoot", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref IEnumerable<GClass2163> __result, Item item)
        {
            // Remove containers in which the item is blacklisted
            List<string> blacklistedContainerTemplateIDs = new List<string>();
            foreach (GClass2163 container in __result)
            {
                bool canUse = ItemCheckActionPatch.PatchPostfix(true, item, container.ParentItem.CurrentAddress);
                if (!canUse)
                {
                    blacklistedContainerTemplateIDs.Add(container.ParentItem.TemplateId);
                }
            }

            __result = __result.Where(i => !blacklistedContainerTemplateIDs.Contains(i.ParentItem.TemplateId));
        }
    }
}
