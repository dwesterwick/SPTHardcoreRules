using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;

namespace HardcoreRules.Helpers
{
    public static class SecureContainerHelpers
    {
        private static List<Item> _secureContainers = new List<Item>();
        public static List<Item> AllSecureContainers
        {
            get
            {
                if (_secureContainers.Count == 0)
                {
                    _secureContainers = getAllSecureContainers();
                }

                return _secureContainers;
            }
        }

        private static List<Item> getAllSecureContainers()
        {
            List<Item> secureContainers = ItemHelpers.AllItems
                .Where(item => item.IsSecureContainer())
                .ToList();

            // Removed secure containers that can't be used by the player (namely the "development" and "boss" secure containers)
            //secureContainers.RemoveAll(c => ConfigUtil.CurrentConfig.SecureContainer.IgnoredSecureContainers.Contains(c.TemplateId.ToString()));

            if (secureContainers.Count == 0)
            {
                throw new InvalidOperationException("Could not create list of secure container ID's");
            }

            return secureContainers;
        }

        public static bool IsSecureContainer(this Item item) => EFT.UI.DragAndDrop.ItemViewFactory.IsSecureContainer(item);

        public static bool IsOrIsInASecureContainer(this Item item)
        {
            foreach (Item parentItem in item.GetAllParentItemsAndSelf())
            {
                if (_secureContainers.Any(c => c.TemplateId == parentItem.TemplateId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
