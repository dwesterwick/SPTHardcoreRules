using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HardcoreRules.Helpers
{
    public static class ItemHelpers
    {
        private static List<Item> _allItems = new List<Item>();
        public static IReadOnlyList<Item> AllItems
        {
            get
            {
                if (_allItems.Count == 0)
                {
                    _allItems = getAllItems();
                }

                return _allItems.AsReadOnly();
            }
        }

        private static List<Item> getAllItems()
        {
            ItemFactory itemFactory = Singleton<ItemFactory>.Instance;
            if (itemFactory == null)
            {
                throw new InvalidOperationException("Could not retrieve ItemFactoryClass instance");
            }

            return itemFactory.CreateAllItemsEver().ToList();
        }
    }
}
