using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Repairing;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Patches
{
    internal class RemoveRepairOptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseItemContextInteractions).GetMethod(nameof(BaseItemContextInteractions.IsInteractive), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ref IResult __result, EItemInfoButton button, ItemUiContext ___ItemUiContext, Item ___Item)
        {
            // No need to continue if the option is already disabled
            if (!__result.Succeed)
            {
                return;
            }

            //LoggingController.LogInfo("Item: " + ___item_0.LocalizedName());

            if ((button == EItemInfoButton.Repair) && !isRepairAllowed(___Item, ___ItemUiContext.Session))
            {
                __result = new FailedResult("Could not find a suitable repair kit");
            }
        }

        private static bool isRepairAllowed(Item item, IEftSession session)
        {
            // Do not allow traders to perform repairs
            foreach (Trader trader in session.Traders)
            {
                trader.Settings.Repair.Availability = false;
            }

            // Build a collection of available repairers for the item
            IRepairStrategy repairerInfo = getRepairerInfo(item, session.RepairController);
            IEnumerable<IRepairer> repairers = repairerInfo.Repairers;

            //LoggingController.LogInfo("Repairers: " + string.Join(", ", repairers.Select(r => r.LocalizedName)));

            if (!repairers.Any())
            {
                return false;
            }

            return true;
        }

        private static IRepairStrategy getRepairerInfo(Item item, RepairController repairController)
        {
            if (item.GetItemComponent<ArmorHolderComponent>() != null)
            {
                return (IRepairStrategy)Activator.CreateInstance(typeof(ArmorRepairStrategy), item, repairController);
            }

            return (IRepairStrategy)Activator.CreateInstance(typeof(DefaultRepairStrategy), item, repairController);
        }
    }
}
