using Aki.Reflection.Patching;
using Comfort.Common;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTHardcoreRules.Patches
{
    public class ItemCheckActionPatch : ModulePatch
    {
        private static List<Item> secureContainers = new List<Item>();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(Item).GetMethod("CheckAction", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static bool PatchPostfix(bool __result, Item __instance, ItemAddress location)
        {
            if ((location == null) || (location.Container == null) || (location.Container.ParentItem == null))
            {
                return __result;
            }

            Item containerItem = location.Item ?? location.Container.ParentItem;

            if (!SPTHardcoreRulesPlugin.ModConfig.SecureContainer.RestrictWhitelistedItems)
            {
                return __result;
            }

            if (secureContainers.Count == 0)
            {
                secureContainers = GetSecureContainerItems();
            }

            bool isItemExamined = Examined(location.Container, __instance);
            bool isItemWhitelisted = IsWhitelisted(__instance, isItemExamined, SPTHardcoreRulesPlugin.IsInRaid);

            bool containerIsSecured = secureContainers.Any(c => c.TemplateId == containerItem.TemplateId);
            if (containerIsSecured)
            {
                return isItemWhitelisted;
            }

            if (!SPTHardcoreRulesPlugin.ModConfig.SecureContainer.MoreRestrictions)
            {
                return __result;
            }

            bool isContainerExamined = Examined(location.Container, containerItem);
            bool isContainerWhitelisted = IsWhitelisted(containerItem, isContainerExamined, SPTHardcoreRulesPlugin.IsInRaid);

            if (!isContainerWhitelisted)
            {
                return __result;
            }

            return isItemWhitelisted;
        }

        public static List<Item> GetSecureContainerItems()
        {
            List<Item> secureContainers = new List<Item>();

            ItemFactory itemFactory = Singleton<ItemFactory>.Instance;
            if (itemFactory == null)
            {
                return secureContainers;
            }

            foreach (Item item in itemFactory.CreateAllItemsEver())
            {
                if (item.Template is SecureContainerTemplateClass)
                {
                    if ((item.Template as SecureContainerTemplateClass).isSecured)
                    {
                        secureContainers.Add(item);
                    }
                }
            }

            secureContainers.RemoveAll(c => SPTHardcoreRulesPlugin.ModConfig.SecureContainer.IgnoredSecureContainers.Contains(c.TemplateId));

            return secureContainers;
        }

        public static bool CanAccept(Item item, List<Item> containerItems)
        {
            foreach (Item containerItem in containerItems)
            {
                if (!containerItem.IsContainer)
                {
                    continue;
                }

                LootItemClass lootItemClass = containerItem as LootItemClass;
                foreach (EFT.InventoryLogic.IContainer container in lootItemClass.Containers)
                {
                    if (container.Filters.CheckItemFilter(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Examined(EFT.InventoryLogic.IContainer container, Item item)
        {
            InventoryControllerClass inventoryControllerClass = container.ParentItem.CurrentAddress.GetOwnerOrNull() as InventoryControllerClass;
            return inventoryControllerClass == null || inventoryControllerClass.Examined(item);
        }

        public static bool IsWhitelisted(Item item, bool isExamined, bool inRaid)
        {
            if (IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.Global))
            {
                return true;
            }

            if (inRaid)
            {
                if (!isExamined && IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InRaid.Uninspected))
                {
                    return true;
                }

                if (isExamined && IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InRaid.Inspected))
                {
                    return true;
                }
            }
            else
            {
                if (IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InHideout))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsItemInWhitelist(Item item, Configuration.Whitelist whitelist)
        {
            if (whitelist.ID_Items.Contains(item.TemplateId))
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
