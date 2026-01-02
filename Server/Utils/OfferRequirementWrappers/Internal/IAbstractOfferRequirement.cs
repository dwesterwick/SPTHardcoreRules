using SPTarkov.Server.Core.Models.Common;

namespace HardcoreRules.Utils.OfferRequirementWrappers.Internal
{
    public interface IAbstractOfferRequirement
    {
        public MongoId Template { get; }
    }
}
