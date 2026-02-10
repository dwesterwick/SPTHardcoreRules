using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.TraderOfferSources.Internal;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.TraderOfferSources
{
    internal class FenceOfferSource : AbstractOfferSource
    {
        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;
        private FenceService _fenceService;

        private TraderConfig _traderConfig;

        private ObjectCache<FenceConfig> _originalFenceConfig = new();

        public FenceOfferSource(LoggingUtil loggingUtil, ConfigServer configServer, FenceService fenceService) : base()
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;
            _fenceService = fenceService;

            _traderConfig = _configServer.GetConfig<TraderConfig>();
        }

        protected override void OnUpdateCache()
        {
            _originalFenceConfig.CacheValueAndThrowIfNull(_traderConfig.Fence);
        }

        protected override void OnRestoreCache()
        {
            _traderConfig.Fence = _originalFenceConfig.GetValueAndThrowIfNull();
        }

        protected override void OnDisable()
        {
            _loggingUtil.Info("Disabling Fence...");

            _traderConfig.Fence.AssortSize = 0;
            _traderConfig.Fence.DiscountOptions.AssortSize = 0;

            _traderConfig.Fence.EquipmentPresetMinMax = new MinMax<int>(0, 0);
            _traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax = new MinMax<int>(0, 0);

            _traderConfig.Fence.WeaponPresetMinMax = new MinMax<int>(0, 0);
            _traderConfig.Fence.DiscountOptions.WeaponPresetMinMax = new MinMax<int>(0, 0);
        }

        protected override void OnEnable()
        {
            _loggingUtil.Info("Enabling Fence...");
        }

        protected override void OnRefresh()
        {
            _fenceService.GenerateFenceAssorts();
        }
    }
}
