using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class GiftOffersUtil
    {
        private GiftsOfferSource Gifts;

        private LoggingUtil _loggingUtil;
        private ConfigServer _configServer;

        public GiftOffersUtil(LoggingUtil loggingUtil, ConfigServer configServer)
        {
            _loggingUtil = loggingUtil;
            _configServer = configServer;

            Gifts = new GiftsOfferSource(_loggingUtil, _configServer);
        }

        public void DisableGifts() => Gifts.Disable();
        public void EnableGifts() => Gifts.Enable();
    }
}
