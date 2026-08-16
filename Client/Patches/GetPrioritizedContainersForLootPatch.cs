using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using HardcoreRules.Helpers;

namespace HardcoreRules.Patches
{
    internal class GetPrioritizedContainersForLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InventoryEquipmentExtension).GetMethod(nameof(InventoryEquipmentExtension.GetPrioritizedContainersForLoot), BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ref IEnumerable<EFT.InventoryLogic.IContainer> __result, Item item)
        {
            __result = __result.Where(container => item.IsAllowedToBePlacedIn(container));
        }
    }
}
