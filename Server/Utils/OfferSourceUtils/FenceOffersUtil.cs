using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Commerce;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class FenceOffersUtil
    {
        private FenceOfferSource Fence;

        private LoggingUtil _loggingUtil;
        private FenceService _fenceService;
        private TraderConfig _traderConfig;

        public FenceOffersUtil(LoggingUtil loggingUtil, FenceService fenceService, TraderConfig traderConfig)
        {
            _loggingUtil = loggingUtil;
            _fenceService = fenceService;
            _traderConfig = traderConfig;

            Fence = new FenceOfferSource(_loggingUtil, _fenceService, _traderConfig);
        }

        public void DisableFence() => Fence.Disable();
        public void EnableFence() => Fence.Enable();
        public void RefreshFenceOffers() => Fence.Refresh();
    }
}
