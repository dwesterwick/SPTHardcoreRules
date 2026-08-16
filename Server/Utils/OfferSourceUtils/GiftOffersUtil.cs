using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class GiftOffersUtil
    {
        private GiftsOfferSource Gifts;

        private LoggingUtil _loggingUtil;
        private GiftsConfig _giftsConfig;

        public GiftOffersUtil(LoggingUtil loggingUtil, GiftsConfig giftsConfig)
        {
            _loggingUtil = loggingUtil;
            _giftsConfig = giftsConfig;

            Gifts = new GiftsOfferSource(_loggingUtil, _giftsConfig);
        }

        public void DisableGifts() => Gifts.Disable();
        public void EnableGifts() => Gifts.Enable();
    }
}
