using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;

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

        public SptProfile? GetSptProfile(MongoId sessionId) => _profileHelper.GetFullProfile(sessionId);
        public PmcData? GetPmcProfile(MongoId sessionId) => _profileHelper.GetPmcProfile(sessionId);

        public string? GetUsername(MongoId sessionId) => GetUsername(GetSptProfile(sessionId));
        public string? GetUsername(SptProfile? sptData) => sptData?.ProfileInfo?.Username;

        public string? GetProfileGameEdition(MongoId sessionId) => GetProfileGameEdition(GetSptProfile(sessionId));
        public string? GetProfileGameEdition(SptProfile? sptData) => sptData?.ProfileInfo?.Edition;

        public bool IsUsingAHardcoreProfile(MongoId sessionId) => IsUsingAHardcoreProfile(GetSptProfile(sessionId));
        public bool IsUsingAHardcoreProfile(SptProfile? sptData) => GetProfileGameEdition(sptData) == Services.AddHardcoreProfileService.HARDCORE_PROFILE_NAME;
    }
}
