using HardcoreRules.Utils.TraderOfferSources.Internal;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.TraderOfferSources
{
    public class FenceOfferSource : IOfferSource
    {
        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;
        private FenceService _fenceService;

        private TraderConfig _traderConfig;

        private ObjectCache<FenceConfig> _originalFenceConfig = new();

        public FenceOfferSource(LoggingUtil loggingUtil, ConfigServer configServer, FenceService fenceService)
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;
            _fenceService = fenceService;

            _traderConfig = _configServer.GetConfig<TraderConfig>();

            UpdateCache();
        }

        private void UpdateCache()
        {
            _originalFenceConfig.CacheValueAndThrowIfNull(_traderConfig.Fence);
        }

        private void RestoreCache()
        {
            _traderConfig.Fence = _originalFenceConfig.GetValueAndThrowIfNull();
        }

        public void Disable()
        {
            _loggingUtil.Info("Disabling Fence...");

            UpdateCache();

            _traderConfig.Fence.AssortSize = 0;
            _traderConfig.Fence.DiscountOptions.AssortSize = 0;

            _traderConfig.Fence.EquipmentPresetMinMax = new MinMax<int>(0, 0);
            _traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax = new MinMax<int>(0, 0);

            _traderConfig.Fence.WeaponPresetMinMax = new MinMax<int>(0, 0);
            _traderConfig.Fence.DiscountOptions.WeaponPresetMinMax = new MinMax<int>(0, 0);
        }

        public void Enable()
        {
            _loggingUtil.Info("Enabling Fence...");

            RestoreCache();
        }

        public void Refresh()
        {
            _fenceService.GenerateFenceAssorts();
        }
    }
}
