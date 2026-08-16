using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources.Internal;
using SPTarkov.Server.Core.Generators.Ragfair;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace HardcoreRules.Utils.OfferSourceUtils.OfferSources
{
    internal class FleaMarketOfferSource : AbstractOfferSource
    {
        private LoggingUtil _loggingUtil;
        private RagfairConfig _ragfairConfig;
        private GlobalTable _globalTable;
        private RagfairOfferGenerator _ragfairOfferGenerator;

        private ObjectCache<Dictionary<string, MinMax<int>>> _originalOfferItemCount = new();
        private ObjectCache<IEnumerable<MaxActiveOfferCount>> _originalMaxActiveOfferCount = new();

        public FleaMarketOfferSource
        (
            LoggingUtil loggingUtil,
            RagfairConfig ragfairConfig,
            GlobalTable globalTable,
            RagfairOfferGenerator ragfairOfferGenerator
        ) : base()
        {
            _loggingUtil = loggingUtil;
            _ragfairConfig = ragfairConfig;
            _globalTable = globalTable;
            _ragfairOfferGenerator = ragfairOfferGenerator;
        }

        protected override void OnUpdateCache()
        {
            _originalOfferItemCount.CacheValueAndThrowIfNull(_ragfairConfig.Dynamic.OfferItemCount);
            _originalMaxActiveOfferCount.CacheValueAndThrowIfNull(_globalTable.Configuration.RagFair.MaxActiveOfferCount);
        }

        protected override void OnRestoreCache()
        {
            _ragfairConfig.Dynamic.OfferItemCount = _originalOfferItemCount.GetValueAndThrowIfNull();
            _globalTable.Configuration.RagFair.MaxActiveOfferCount = _originalMaxActiveOfferCount.GetValueAndThrowIfNull();
        }

        protected override void OnDisable()
        {
            _loggingUtil.Info("Disabling flea market...");

            foreach (MinMax<int> limits in _ragfairConfig.Dynamic.OfferItemCount.Values)
            {
                limits.Min = 0;
                limits.Max = 0;
            }

            foreach (MaxActiveOfferCount offerCount in _globalTable.Configuration.RagFair.MaxActiveOfferCount)
            {
                offerCount.Count = 0;
            }
        }

        protected override void OnEnable()
        {
            _loggingUtil.Info("Enabling flea market...");
        }

        protected override void OnRefresh()
        {
            _loggingUtil.Info("Refreshing flea market offers...");
            _ragfairOfferGenerator.GenerateDynamicOffers();
        }
    }
}
