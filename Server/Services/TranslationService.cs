using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    public class TranslationService : AbstractService
    {
        private DatabaseService _databaseService;
        private LocaleService _localeService;
        private ServerLocalisationService _serverLocalisationService;

        private Dictionary<string, string> _cachedTranslations = new();

        public TranslationService
        (
            LoggingUtil logger,
            ConfigUtil config,
            DatabaseService databaseService,
            LocaleService localeService,
            ServerLocalisationService serverLocalisationService
        ) : base(logger, config)
        {
            _databaseService = databaseService;
            _localeService = localeService;
            _serverLocalisationService = serverLocalisationService;
        }

        public string GetLocalisedValue(string key)
        {
            if ( _cachedTranslations.ContainsKey(key))
            {
                return _cachedTranslations[key];
            }

            string translation = _serverLocalisationService.GetLocalisedValue(key);
            _cachedTranslations.Add(key, translation);

            return translation;
        }

        public string GetLocalisedTraderName(Trader trader) => GetLocalisedValue($"{trader.Base.Id} Nickname");
        public string GetLocalisedItemName(TemplateItem item) => GetLocalisedValue($"{item.Id} Name");

        public bool TryGetNewTranslation(string key, string? locale, out string? translation, out string localeUsed)
        {
            translation = null;
            localeUsed = locale ?? _localeService.GetDesiredServerLocale();

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!Config.Translations.ContainsKey(key))
            {
                return false;
            }

            if (!Config.Translations[key].ContainsKey(localeUsed))
            {
                return false;
            }

            translation = Config.Translations[key][localeUsed];
            return true;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            AddNewTranslations();
        }

        private void AddNewTranslations()
        {
            int maxTranslationsAdded = 0;

            Dictionary<string, string> languages = _databaseService.GetTables().Locales.Languages;
            foreach (string locale in languages.Keys)
            {
                int translationsAddedForLocale = AddNewTranslationsForLocale(locale);
                maxTranslationsAdded = Math.Max(maxTranslationsAdded, translationsAddedForLocale);
            }

            Logger.Info($"Added {maxTranslationsAdded} new translation(s)");
        }

        private int AddNewTranslationsForLocale(string locale)
        {
            int translationsAdded = 0;

            Dictionary<string, string> newTranslations = Config.Translations
                    .Where(translation => translation.Value.ContainsKey(locale))
                    .ToDictionary(translation => translation.Key, translation => translation.Value[locale]);

            if (newTranslations.Any())
            {
                Dictionary<string, string> translations = _localeService.GetLocaleDb(locale);

                foreach (string key in newTranslations.Keys)
                {
                    translations.Add(key, newTranslations[key]);
                    translationsAdded++;
                }
            }

            return translationsAdded;
        }
    }
}
