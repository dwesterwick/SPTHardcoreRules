using HardcoreRules.Utils.TraderOfferSources.Internal;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.TraderOfferSources
{
    public class FleaMarketOfferSource : IOfferSource
    {
        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;
        private DatabaseService _databaseService;
        private RagfairOfferGenerator _ragfairOfferGenerator;

        private RagfairConfig _ragfairConfig;

        private ObjectCache<Dictionary<string, MinMax<int>>> _originalOfferItemCount = new();
        private ObjectCache<IEnumerable<MaxActiveOfferCount>> _originalMaxActiveOfferCount = new();

        public FleaMarketOfferSource
        (
            LoggingUtil loggingUtil,
            ConfigServer configServer,
            DatabaseService databaseService,
            RagfairOfferGenerator ragfairOfferGenerator
        )
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;
            _databaseService = databaseService;
            _ragfairOfferGenerator = ragfairOfferGenerator;

            _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

            UpdateCache();
        }

        private void UpdateCache()
        {
            _originalOfferItemCount.CacheValueAndThrowIfNull(_ragfairConfig.Dynamic.OfferItemCount);
            _originalMaxActiveOfferCount.CacheValueAndThrowIfNull(_databaseService.GetTables().Globals.Configuration.RagFair.MaxActiveOfferCount);
        }

        private void RestoreCache()
        {
            _ragfairConfig.Dynamic.OfferItemCount = _originalOfferItemCount.GetValueAndThrowIfNull();
            _databaseService.GetTables().Globals.Configuration.RagFair.MaxActiveOfferCount = _originalMaxActiveOfferCount.GetValueAndThrowIfNull();
        }

        public void Disable()
        {
            _loggingUtil.Info("Disabling flea market...");

            UpdateCache();

            foreach (MinMax<int> limits in _ragfairConfig.Dynamic.OfferItemCount.Values)
            {
                limits.Min = 0;
                limits.Max = 0;
            }

            foreach (MaxActiveOfferCount offerCount in _databaseService.GetTables().Globals.Configuration.RagFair.MaxActiveOfferCount)
            {
                offerCount.Count = 0;
            }
        }

        public void Enable()
        {
            _loggingUtil.Info("Enabling flea market...");

            RestoreCache();
        }

        public void Refresh()
        {
            _loggingUtil.Info("Refreshing flea market offers...");
            _ragfairOfferGenerator.GenerateDynamicOffers();
        }
    }
}
