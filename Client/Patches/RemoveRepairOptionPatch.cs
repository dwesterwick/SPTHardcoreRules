using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using HardcoreRules.Utils;

namespace HardcoreRules.Patches
{
    public class RemoveRepairOptionPatch : ModulePatch
    {
        private static Type _repairerInfoInterface = null!;
        private static Type _repairerInfoArmor = null!;
        private static Type _repairerInfoWeapon = null!;
        private static PropertyInfo _repairersField = null!;

        protected override MethodBase GetTargetMethod()
        {
            string methodName = "IsInteractive";
            Type targetType = findTargetType(methodName, typeof(EItemInfoButton), "IsOwnedByPlayer");

            Singleton<LoggingUtil>.Instance.LogInfo("Found target type for RemoveRepairOptionPatch: " + targetType);

            findRepairerTypes("AddRepairKitToRepairers");
            _repairersField = AccessTools.Property(_repairerInfoInterface, "Repairers");

            Singleton<LoggingUtil>.Instance.LogInfo("Found repairer-info interface type for RemoveRepairOptionPatch: " + _repairerInfoInterface);
            Singleton<LoggingUtil>.Instance.LogInfo("Found armor repairer info type for RemoveRepairOptionPatch: " + _repairerInfoArmor);
            Singleton<LoggingUtil>.Instance.LogInfo("Found weapon repairer info type for RemoveRepairOptionPatch: " + _repairerInfoWeapon);

            return targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(ref IResult __result, EItemInfoButton button, ItemUiContext ___ItemUiContext_0, Item ___Item_0_1)
        {
            // No need to continue if the option is already disabled
            if (!__result.Succeed)
            {
                return;
            }

            //LoggingController.LogInfo("Item: " + ___item_0.LocalizedName());

            if ((button == EItemInfoButton.Repair) && !isRepairAllowed(___Item_0_1, ___ItemUiContext_0.Session))
            {
                __result = new FailedResult("Could not find a suitable repair kit");
            }
        }

        private static bool isRepairAllowed(Item item, ISession session)
        {
            // Do not allow traders to perform repairs
            foreach (TraderClass trader in session.Traders)
            {
                trader.Settings.Repair.Availability = false;
            }

            // Build a collection of available repairers for the item
            object repairerInfo = getRepairerInfo(item, session.RepairController);
            IEnumerable<IRepairer> repairers = (IEnumerable<IRepairer>)_repairersField.GetValue(repairerInfo);

            //LoggingController.LogInfo("Repairers: " + string.Join(", ", repairers.Select(r => r.LocalizedName)));

            if (!repairers.Any())
            {
                return false;
            }

            return true;
        }

        private static object getRepairerInfo(Item item, RepairControllerClass repairController)
        {
            if (item.GetItemComponent<ArmorHolderComponent>() != null)
            {
                return Activator.CreateInstance(_repairerInfoArmor, item, repairController);
            }

            return Activator.CreateInstance(_repairerInfoWeapon, item, repairController);
        }

        private static Type findTargetType(string methodName, Type methodParameterType, string requiredPropertyNameInType)
        {
            Type[] targetTypeOptions = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => t.GetProperties().Any(p => p.Name.Contains(requiredPropertyNameInType)))
                .Where(t => t
                    .GetMethods()
                    .Any(m => m.Name.Contains(methodName) && m.GetParameters().All(p => p.ParameterType == methodParameterType)))
                .ToArray();

            if (targetTypeOptions.Length != 1)
            {
                throw new TypeLoadException("Found " + targetTypeOptions.Length + " types containing method " + methodName + ", parameter type " + methodParameterType + ", and property " + requiredPropertyNameInType + ": " + string.Join(", ", targetTypeOptions.Select(t => t.Name)));
            }

            return targetTypeOptions[0];
        }

        private static void findRepairerTypes(string methodName)
        {
            Type[] repairerInfoTypes = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => t.GetMethods().Any(m => m.Name.Contains(methodName)))
                .ToArray();

            int expectedMatches = 3;
            if (repairerInfoTypes.Length != expectedMatches)
            {
                throw new TypeLoadException("Found " + repairerInfoTypes.Length + " types containing method " + methodName + " but expected " + expectedMatches);
            }

            IEnumerable<Type> repairerInfoInterfaceMatches = repairerInfoTypes.Where(t => t.IsInterface);
            if (repairerInfoInterfaceMatches.Count() != 1)
            {
                throw new TypeLoadException("Could not find repairer info interface");
            }
            _repairerInfoInterface = repairerInfoInterfaceMatches.First();

            int maxMethodCount = repairerInfoTypes.Max(t => t.GetMethods().Count());
            _repairerInfoArmor = repairerInfoTypes.Single(t => t.GetMethods().Count() == maxMethodCount);
            _repairerInfoWeapon = repairerInfoTypes.Single(t => (t != _repairerInfoInterface) && (t != _repairerInfoArmor));
        }
    }
}
