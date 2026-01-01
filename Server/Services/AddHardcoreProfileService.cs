using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    public class AddHardcoreProfileService : AbstractService
    {
        public const string HARDCORE_PROFILE_NAME = "Hardcore Playthrough";

        private const string ZERO_TO_HERO_PROFILE_NAME = "SPT Zero to hero";
        private const string HARDCORE_PROFILE_DESCRIPTION_LOCALE_KEY = "launcher-profile_hardcoreplaythrough";
        private const string FALLBACK_LOCALE = "en";
        private const bool LAUNCHER_USES_NEW_TRANSLATIONS = false;

        private DatabaseService _databaseService;
        private NewTranslationsService _newTranslationsService;

        public AddHardcoreProfileService
        (
            LoggingUtil logger,
            ConfigUtil config,
            DatabaseService databaseService,
            NewTranslationsService newTranslationsService
        ) : base(logger, config)
        {
            _databaseService = databaseService;
            _newTranslationsService = newTranslationsService;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            AddHardcoreProfileTemplate();
        }

        private void AddHardcoreProfileTemplate()
        {
            string profileTemplateToClone = ZERO_TO_HERO_PROFILE_NAME;
            ProfileSides? hardcoreProfileTemplate = CreateProfileTemplateClone(profileTemplateToClone);
            if (hardcoreProfileTemplate?.Bear?.Character?.Inventory?.Items == null)
            {
                throw new InvalidOperationException($"Could not create clone of {profileTemplateToClone} profile template");
            }

            hardcoreProfileTemplate.Bear.Character.Inventory.Items.PopLast();
            hardcoreProfileTemplate.Usec!.Character!.Inventory!.Items!.PopLast();

            hardcoreProfileTemplate.DescriptionLocaleKey = GetProfileDescription();

            _databaseService.GetTables().Templates.Profiles.Add(HARDCORE_PROFILE_NAME, hardcoreProfileTemplate);

            Logger.Info("Created hardcore profile template");
        }

        private ProfileSides? CreateProfileTemplateClone(string profileTemplateName)
        {
            if (!_databaseService.GetTables().Templates.Profiles.TryGetValue(profileTemplateName, out ProfileSides? profile))
            {
                return null;
            }

            return FastCloner.FastCloner.DeepClone(profile);
        }

        private string GetProfileDescription()
        {
            if (LAUNCHER_USES_NEW_TRANSLATIONS || !TryGetTranslatedProfileDescription(out string? translation) || string.IsNullOrEmpty(translation))
            {
                return HARDCORE_PROFILE_DESCRIPTION_LOCALE_KEY;
            }

            return translation;
        }

        private bool TryGetTranslatedProfileDescription(out string? translation)
        {
            translation = null;

            string? locale = null;
            bool foundTranslation = _newTranslationsService.TryGetNewTranslation(HARDCORE_PROFILE_DESCRIPTION_LOCALE_KEY, locale, out translation, out string localeUsed);
            if (foundTranslation)
            {
                return true;
            }

            if (localeUsed != FALLBACK_LOCALE)
            {
                locale = FALLBACK_LOCALE;
                Logger.Warning($"Could not find {localeUsed} translation for hardcore profile description. Falling back to {locale}...");

                foundTranslation = _newTranslationsService.TryGetNewTranslation(HARDCORE_PROFILE_DESCRIPTION_LOCALE_KEY, locale, out translation, out localeUsed);
                if (foundTranslation)
                {
                    return true;
                }
            }

            Logger.Error($"Could not find a translation for hardcore profile description.");
            return false;
        }
    }
}
