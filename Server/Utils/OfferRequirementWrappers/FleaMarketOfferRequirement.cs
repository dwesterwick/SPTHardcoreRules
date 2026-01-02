using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;

namespace HardcoreRules.Utils.OfferRequirementWrappers
{
    public class FleaMarketOfferRequirement : Internal.IAbstractOfferRequirement
    {
        private OfferRequirement _requirement;

        public MongoId Template => _requirement.TemplateId;

        public FleaMarketOfferRequirement(OfferRequirement requirement)
        {
            _requirement = requirement;
        }
    }
}
