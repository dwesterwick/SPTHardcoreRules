using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources.Internal;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace HardcoreRules.Utils.OfferSourceUtils.OfferSources
{
    internal class GiftsOfferSource : AbstractOfferSource
    {
        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;

        private GiftsConfig _giftsConfig;

        private ObjectCache<Dictionary<string, Gift>> _originalGifts = new();

        public GiftsOfferSource(LoggingUtil loggingUtil, ConfigServer configServer) : base()
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;

            _giftsConfig = _configServer.GetConfig<GiftsConfig>();
        }

        protected override void OnUpdateCache()
        {
            _originalGifts.CacheValueAndThrowIfNull(_giftsConfig.Gifts);
        }

        protected override void OnRestoreCache()
        {
            _giftsConfig.Gifts = _originalGifts.GetValueAndThrowIfNull();
        }

        protected override void OnDisable()
        {
            _loggingUtil.Info("Disabling gifts...");

            foreach (Gift gift in _giftsConfig.Gifts.Values)
            {
                gift.Items.Clear();
            }
        }

        protected override void OnEnable()
        {
            _loggingUtil.Info("Enabling gifts...");
        }

        protected override void OnRefresh()
        {
            throw new NotImplementedException();
        }
    }
}
