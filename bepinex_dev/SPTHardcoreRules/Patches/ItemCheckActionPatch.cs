using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT.InventoryLogic;
using SPTHardcoreRules.Models;
using SPTHardcoreRules.Controllers;

namespace SPTHardcoreRules.Patches
{
    public class ItemCheckActionPatch : ModulePatch
    {
        private static List<Item> secureContainers = new List<Item>();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(Item).GetMethod("CheckAction", BindingFlags.Public | BindingFlags.Instance);
        }

        internal static GStruct447 GetCheckActionResult(Item __instance, ItemAddress location)
        {
            GStruct447 result = default(GStruct447);
            PatchPrefix(ref result, __instance, location);

            return result;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref GStruct447 __result, Item __instance, ItemAddress location)
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

            Item containerItem = location.Container.ParentItem;

            if (!Controllers.ConfigController.Config.SecureContainer.UseModWhitelists)
            {
                return true;
            }

            // This should only be run once to generate the list of secure containers
            if (secureContainers.Count == 0)
            {
                secureContainers = GetSecureContainerItems();
            }

            // See if the target container is or is in a secure container
            bool targetContainerIsSecured = false;
            foreach (Item item in containerItem.GetAllParentItemsAndSelf())
            {
                targetContainerIsSecured |= secureContainers.Any(c => c.TemplateId == item.TemplateId);
            }
            if (!targetContainerIsSecured)
            {
                return true;
            }

            // Check if the item and all items contained within it are whitelisted
            bool allItemsWhitelisted = true;
            foreach(Item item in __instance.GetAllItems())
            {
                bool isContainedItemExamined = IsExamined(location.Container, item);
                bool isContainedItemWhitelisted = IsWhitelisted(item, isContainedItemExamined, CurrentRaidSettings.IsInRaid);
                allItemsWhitelisted &= isContainedItemWhitelisted;
            }

            if (allItemsWhitelisted)
            {
                LoggingController.LogInfo("Result for putting " + __instance.LocalizedName() + " in " + containerItem.LocalizedName() + ": " + __result.Succeeded);

                return true;
            }

            __result = new SecureContainerRestrictionError(__instance);
            LoggingController.LogInfo("Result for putting " + __instance.LocalizedName() + " in " + containerItem.LocalizedName() + ": " + __result.Succeeded);

            return false;
        }

        public static List<Item> GetSecureContainerItems()
        {
            List<Item> secureContainers = new List<Item>();

            ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance;
            if (itemFactory == null)
            {
                return secureContainers;
            }

            // Find all possible secure containers
            foreach (Item item in itemFactory.CreateAllItemsEver())
            {
                if (!EFT.UI.DragAndDrop.ItemViewFactory.IsSecureContainer(item))
                {
                    continue;
                }

                secureContainers.Add(item);
            }

            // Removed secure containers that can't be used by the player (namely the "development" and "boss" secure containers)
            secureContainers.RemoveAll(c => Controllers.ConfigController.Config.SecureContainer.IgnoredSecureContainers.Contains(c.TemplateId.ToString()));

            return secureContainers;
        }

        // Copied from EFT.InventoryLogic.Examined(Item item)
        public static bool IsExamined(EFT.InventoryLogic.IContainer container, Item item)
        {
            InventoryController inventoryControllerClass = container.ParentItem.CurrentAddress.GetOwnerOrNull() as InventoryController;
            return inventoryControllerClass == null || inventoryControllerClass.Examined(item);
        }

        public static bool IsWhitelisted(Item item, bool isExamined, bool inRaid)
        {
            if (IsItemInWhitelist(item, Controllers.ConfigController.Config.SecureContainer.Whitelists.Global))
            {
                return true;
            }

            if (inRaid)
            {
                if (!isExamined && IsItemInWhitelist(item, Controllers.ConfigController.Config.SecureContainer.Whitelists.InRaid.Uninspected))
                {
                    return true;
                }

                if (isExamined && IsItemInWhitelist(item, Controllers.ConfigController.Config.SecureContainer.Whitelists.InRaid.Inspected))
                {
                    return true;
                }
            }
            else
            {
                if (IsItemInWhitelist(item, Controllers.ConfigController.Config.SecureContainer.Whitelists.InHideout))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsItemInWhitelist(Item item, Configuration.Whitelist whitelist)
        {
            if (whitelist.ID_Items.Contains(item.TemplateId.ToString()))
            {
                return true;
            }

            foreach (string parentID in whitelist.ID_Parents)
            {
                if (item.Template.IsChildOf(parentID))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
