using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class FenceOffersUtil
    {
        private FenceOfferSource Fence;

        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;
        private FenceService _fenceService;

        public FenceOffersUtil(LoggingUtil loggingUtil, ConfigServer configServer, FenceService fenceService)
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;
            _fenceService = fenceService;

            Fence = new FenceOfferSource(_loggingUtil, _configServer, _fenceService);
        }

        public void DisableFence() => Fence.Disable();
        public void EnableFence() => Fence.Enable();
        public void RefreshFenceOffers() => Fence.Refresh();
    }
}
