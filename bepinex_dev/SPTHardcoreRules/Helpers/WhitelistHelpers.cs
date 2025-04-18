﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using SPTHardcoreRules.Controllers;
using SPTHardcoreRules.Models;

namespace SPTHardcoreRules.Helpers
{
    public static class WhitelistHelpers
    {
        public static bool IsAllowedToBePlacedIn(this Item item, EFT.InventoryLogic.IContainer container)
        {
            if ((container == null) || (container.ParentItem == null))
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (!Controllers.ConfigController.Config.SecureContainer.UseModWhitelists)
            {
                return true;
            }

            if (!container.ParentItem.IsOrIsInASecureContainer())
            {
                return true;
            }

            InventoryController inventoryController = container.GetInventoryController();
            if (item.AreAllContainedItemsWhitelisted(inventoryController))
            {
                return true;
            }

            return false;
        }

        public static InventoryController GetInventoryController(this EFT.InventoryLogic.IContainer container)
        {
            InventoryController inventoryController = container.ParentItem.Owner as InventoryController;
            if (inventoryController == null)
            {
                LoggingController.LogWarning("Could not retrieve InventoryController for " + container.ParentItem.LocalizedName());
            }

            return inventoryController;
        }

        public static bool IsExamined(this Item item, InventoryController inventoryController)
        {
            return inventoryController == null || inventoryController.Examined(item);
        }

        public static bool IsWhitelisted(this Item item, bool isExamined)
        {
            return item.IsWhitelisted(isExamined, CurrentRaidSettings.IsInRaid);
        }

        public static bool IsWhitelisted(this Item item, bool isExamined, bool inRaid)
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

        public static bool IsItemInWhitelist(this Item item, Configuration.Whitelist whitelist)
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

        public static bool AreAllContainedItemsWhitelisted(this Item item, InventoryController inventoryController)
        {
            foreach (Item childItem in item.GetAllItems())
            {
                bool isContainedItemExamined = childItem.IsExamined(inventoryController);
                if (!childItem.IsWhitelisted(isContainedItemExamined))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
