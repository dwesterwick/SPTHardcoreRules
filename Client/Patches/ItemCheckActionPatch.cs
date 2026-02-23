using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using HardcoreRules.Helpers;
using HardcoreRules.Models;

namespace HardcoreRules.Patches
{
    internal class ItemCheckActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Item).GetMethod(nameof(Item.CheckAction), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref GStruct155 __result, Item __instance, ItemAddress location)
        {
            // Don't apply restrictions to Scavs because they don't have secure containers
            if (CurrentRaidSettings.SelectedSide == EFT.ESideType.Savage)
            {
                return true;
            }

            if ((location == null) || (location.Container == null) || (location.Container.ParentItem == null))
            {
                return true;
            }

            if (__instance.IsAllowedToBePlacedIn(location.Container))
            {
                return true;
            }

            __result = new SecureContainerRestrictionError(__instance);
            return false;
        }
    }
}
