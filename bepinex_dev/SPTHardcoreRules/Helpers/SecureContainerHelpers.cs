using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.InventoryLogic;

namespace SPTHardcoreRules.Helpers
{
    public static class SecureContainerHelpers
    {
        private static List<Item> secureContainers = new List<Item>();

        public static List<Item> GetAllSecureContainers()
        {
            if (secureContainers.Count > 0)
            {
                return secureContainers;
            }

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

        public static bool IsOrIsInASecureContainer(this Item item)
        {
            List<Item> secureContainers = GetAllSecureContainers();

            foreach (Item containedItem in item.GetAllParentItemsAndSelf())
            {
                if (secureContainers.Any(c => c.TemplateId == containedItem.TemplateId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
