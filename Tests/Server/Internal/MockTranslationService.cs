using HardcoreRules.Services;
using HardcoreRules.Utils;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcoreRules.Server.Internal
{
    internal class MockTranslationService : TranslationService
    {
        public MockTranslationService
        (
            LoggingUtil logger,
            ConfigUtil config,
            LocaleTable localeTable,
            LocaleService localeService,
            ServerLocalisationService serverLocalisationService
        ) : base(logger, config, localeTable, localeService, serverLocalisationService)
        {

        }

        public override string GetLocalisedValue(string key)
        {
            // Need to temporarily switch directories because translations are lazy loaded
            return RunFromSptInstallDirectoryService.RunFromSptInstallDirectory(base.GetLocalisedValue, key);
        }
    }
}
