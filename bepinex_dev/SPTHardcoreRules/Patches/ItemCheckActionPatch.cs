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
        private const string ID_Pockets = "627a4e6b255f7527fb05a0f6";
        private const string ID_Stash = "566abbb64bdc2d144c8b457d";
        private const string ID_DevSecureContainer = "5c0a5a5986f77476aa30ae64";
        private const string ID_BossSecureContrainer = "5c0a794586f77461c458f892";

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
            Item item = __instance;

            //Logger.LogInfo("Checking " + __instance.LocalizedName() + " for container " + containerItem.LocalizedName());

            if (!SPTHardcoreRulesPlugin.ModConfig.SecureContainer.RestrictWhitelistedItems)
            {
                return __result;
            }

            if (secureContainers.Count == 0)
            {
                secureContainers = GetSecureContainerItems();
            }

            bool isItemExamined = Examined(location.Container, item);
            bool isItemWhitelisted = IsWhitelisted(item, isItemExamined, SPTHardcoreRulesPlugin.IsInRaid);
            Logger.LogInfo("Item " + item.LocalizedName() + " whitelisted: " + isItemWhitelisted);

            bool containerIsSecured = secureContainers.Any(c => c.TemplateId == containerItem.TemplateId);
            if (containerIsSecured)
            {
                Logger.LogInfo("Container " + containerItem.LocalizedName() + " (" + containerItem.TemplateId + ") is secured");
                return isItemWhitelisted;
            }

            if (!SPTHardcoreRulesPlugin.ModConfig.SecureContainer.MoreRestrictions)
            {
                return __result;
            }

            //bool containerItemAllowedInSecureContainers = CanAccept(container.ParentItem, secureContainers);

            bool isContainerExamined = Examined(location.Container, containerItem);
            bool isContainerWhitelisted = IsWhitelisted(containerItem, isContainerExamined, SPTHardcoreRulesPlugin.IsInRaid);
            if (isContainerWhitelisted)
            {
                Logger.LogInfo("Container " + containerItem.LocalizedName() + " whitelisted: " + isContainerWhitelisted);
            }

            if (!isContainerWhitelisted)
            {
                Logger.LogInfo("Returning " + __result);
                return __result;
            }

            Logger.LogInfo("Returning " + isItemWhitelisted);
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

            secureContainers.RemoveAll(c => c.TemplateId == ID_BossSecureContrainer || c.TemplateId == ID_DevSecureContainer);

            Logger.LogInfo("Found " + secureContainers.Count + " secure containers...");
            foreach (Item item in secureContainers)
            {
                Logger.LogInfo("Found " + secureContainers.Count + " secure containers..." + item.LocalizedName() + " (" + item.TemplateId + ")");
            }

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
                        Logger.LogInfo("Item " + item.LocalizedName() + " can be put in " + containerItem.LocalizedName());
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
                Logger.LogInfo("Global whitelist");
                return true;
            }

            if (inRaid)
            {
                if (!isExamined && IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InRaid.Uninspected))
                {
                    Logger.LogInfo("In-raid and not examined");
                    return true;
                }

                if (isExamined && IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InRaid.Inspected))
                {
                    Logger.LogInfo("In-raid and examined");
                    return true;
                }
            }
            else
            {
                if (IsItemInWhitelist(item, SPTHardcoreRulesPlugin.ModConfig.SecureContainer.Whitelists.InHideout))
                {
                    Logger.LogInfo("In hideout");
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
