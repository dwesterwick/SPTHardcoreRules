using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules.Patches
{
    public class RemoveRepairOptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            string methodName = "IsInteractive";
            Type targetType = findTargetType(methodName, typeof(EItemInfoButton), "IsOwnedByPlayer");

            LoggingController.LogInfo("Found type for RemoveRepairOptionPatch: " + targetType);

            return targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref IResult __result, EItemInfoButton button, ItemUiContext ___itemUiContext_0, Item ___item_0)
        {
            if (button != EItemInfoButton.Repair)
            {
                return;
            }

            LoggingController.LogInfo("Repair IsInteractive: " + __result.Succeed);
            LoggingController.LogInfo("Item: " + ___item_0.LocalizedName());

            // No need to continue if the item can't be repaired
            if (!__result.Succeed)
            {
                return;
            }

            // Do not allow traders to perform repairs
            foreach (TraderClass trader in ___itemUiContext_0.Session.Traders)
            {
                trader.Settings.Repair.Availability = false;
            }

            GInterface34 repairData = RepairWindow_method_2(___item_0, ___itemUiContext_0.Session.RepairController);
            LoggingController.LogInfo("Repairers: " + string.Join(", ", repairData.Repairers.Select(r => r.LocalizedName)));

            if (!repairData.Repairers.Any())
            {
                __result = new FailedResult("Could not find a suitable repair kit");
            }
        }

        public static GInterface34 RepairWindow_method_2(Item item, RepairControllerClass repairController)
        {
            if (item.GetItemComponent<ArmorHolderComponent>() != null)
            {
                return new GClass805(item, repairController);
            }

            return new GClass804(item, repairController);
        }

        private static Type findTargetType(string methodName, Type methodParameterType, string requiredPropertyNameInType)
        {
            List<Type> targetTypeOptions = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => t.GetProperties().Any(p => p.Name.Contains(requiredPropertyNameInType)))
                .Where(t => t
                    .GetMethods()
                    .Any(m => m.Name.Contains(methodName) && m.GetParameters().All(p => p.ParameterType == methodParameterType)))
                .ToList();

            if (targetTypeOptions.Count != 1)
            {
                throw new TypeLoadException("Found " + targetTypeOptions.Count + " types containing method " + methodName + ", parameter type " + methodParameterType + ", and property " + requiredPropertyNameInType + ": " + string.Join(", ", targetTypeOptions));
            }

            return targetTypeOptions[0];
        }
    }
}
