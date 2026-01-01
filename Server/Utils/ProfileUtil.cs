using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace HardcoreRules.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class ProfileUtil
    {
        private ProfileHelper _profileHelper;

        public ProfileUtil(ProfileHelper profileHelper)
        {
            _profileHelper = profileHelper;
        }

        public PmcData? GetPmcProfile(MongoId sessionId) => _profileHelper.GetPmcProfile(sessionId);

        public string? GetProfileGameVersion(MongoId sessionId) => GetProfileGameVersion(GetPmcProfile(sessionId));
        public string? GetProfileGameVersion(PmcData? pmcData) => pmcData?.Info?.GameVersion;
    }
}
