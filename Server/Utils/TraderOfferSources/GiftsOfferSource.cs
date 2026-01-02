using HardcoreRules.Utils.TraderOfferSources.Internal;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace HardcoreRules.Utils.TraderOfferSources
{
    public class GiftsOfferSource : IOfferSource
    {
        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;

        private GiftsConfig _giftsConfig;

        private ObjectCache<GiftsConfig> _originalGiftsConfig = new();

        public GiftsOfferSource(LoggingUtil loggingUtil, ConfigServer configServer)
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;

            _giftsConfig = _configServer.GetConfig<GiftsConfig>();

            UpdateCache();
        }

        private void UpdateCache()
        {
            _originalGiftsConfig.CacheValueAndThrowIfNull(_giftsConfig);
        }

        private void RestoreCache()
        {
            _giftsConfig = _originalGiftsConfig.GetValueAndThrowIfNull();
        }

        public void Disable()
        {
            _loggingUtil.Info("Disabling gifts...");

            UpdateCache();

            _giftsConfig.Gifts.Clear();
        }

        public void Enable()
        {
            _loggingUtil.Info("Enabling gifts...");

            RestoreCache();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }
    }
}
