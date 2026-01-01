using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;

namespace HardcoreRules.Models
{
    internal class SecureContainerRestrictionError : InventoryError
    {
        public Item Item { get; private set; }

        public SecureContainerRestrictionError(Item item)
        {
            Item = item;
        }

        public override string ToString()
        {
            return $"Cannot place {Item.LocalizedName()} into your secure container per Hardcore Rules";
        }

        public override string GetLocalizedDescription() => ToString();
    }
}
