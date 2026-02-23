using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace HardcoreRules.Utils.OfferRequirementWrappers
{
    public class BarterSchemeRequirement : Internal.IAbstractOfferRequirement
    {
        private BarterScheme _barterScheme;

        public MongoId Template => _barterScheme.Template;

        public BarterSchemeRequirement(BarterScheme barterScheme)
        {
            _barterScheme = barterScheme;
        }
    }
}
