using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources.Internal;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Commerce;

namespace HardcoreRules.Utils.OfferSourceUtils.OfferSources
{
    internal class FenceOfferSource : AbstractOfferSource
    {
        private LoggingUtil _loggingUtil;
        private FenceService _fenceService;
        private TraderConfig _traderConfig;

        private ObjectCache<FenceConfig> _originalFenceConfig = new();

        public FenceOfferSource(LoggingUtil loggingUtil, FenceService fenceService, TraderConfig traderConfig) : base()
        {
            _loggingUtil = loggingUtil;
            _fenceService = fenceService;
            _traderConfig = traderConfig;
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
