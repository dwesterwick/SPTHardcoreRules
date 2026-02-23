using HardcoreRules.Services;
using HardcoreRules.Utils;
using SPTarkov.Server.Core.Services;
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
            DatabaseService databaseService,
            LocaleService localeService,
            ServerLocalisationService serverLocalisationService
        ) : base(logger, config, databaseService, localeService, serverLocalisationService)
        {

        }

        public override string GetLocalisedValue(string key)
        {
            return RunFromSptInstallDirectoryService.RunFromSptInstallDirectory(base.GetLocalisedValue, key);
        }
    }
}
