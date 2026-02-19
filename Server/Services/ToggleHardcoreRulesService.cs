using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostSptModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    internal class ToggleHardcoreRulesService : AbstractService
    {
        public static bool HardcoreRulesEnabled { get; private set; } = false;

        private TraderOffersUtil _traderOffersUtil;
        private GiftOffersUtil _giftOffersUtil;
        private FenceOffersUtil _fenceOffersUtil;
        private FleaMarketOffersUtil _fleaMarketOffersUtil;

        public ToggleHardcoreRulesService
        (
            LoggingUtil logger,
            ConfigUtil config,
            TraderOffersUtil traderOffersUtil,
            GiftOffersUtil giftOffersUtil,
            FenceOffersUtil fenceOffersUtil,
            FleaMarketOffersUtil fleaMarketOffersUtil
        ) : base(logger, config)
        {
            _traderOffersUtil = traderOffersUtil;
            _giftOffersUtil = giftOffersUtil;
            _fenceOffersUtil = fenceOffersUtil;
            _fleaMarketOffersUtil = fleaMarketOffersUtil;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            
        }

        public void ToggleHardcoreRules(bool enableHardcoreRules)
        {
            if (!enableHardcoreRules)
            {
                Logger.Warning("Not using a hardcore profile");
            }

            if (enableHardcoreRules == HardcoreRulesEnabled)
            {
                return;
            }

            if (enableHardcoreRules)
            {
                EnableHardcoreRules();
            }
            else
            {
                DisableHardcoreRules();
            }
        }

        private void EnableHardcoreRules()
        {
            Logger.Info("Enabling hardcore rules...");

            if (!Config.CurrentConfig.Services.FleaMarket.Enabled)
            {
                _fleaMarketOffersUtil.DisableFleaMarket();
            }

            if (Config.CurrentConfig.Traders.DisableFence)
            {
                _fenceOffersUtil.DisableFence();
            }

            if (Config.CurrentConfig.Traders.DisableStartingGifts)
            {
                _giftOffersUtil.DisableGifts();
            }

            _fenceOffersUtil.RefreshFenceOffers();
            _traderOffersUtil.RemoveBannedTraderOffers();
            _fleaMarketOffersUtil.RefreshFleaMarketOffers();

            HardcoreRulesEnabled = true;
            Logger.Info("Enabling hardcore rules...done.");
        }

        private void DisableHardcoreRules()
        {
            Logger.Info("Disabling hardcore rules...");

            if (!Config.CurrentConfig.Services.FleaMarket.Enabled)
            {
                _fleaMarketOffersUtil.EnableFleaMarket();
            }

            if (Config.CurrentConfig.Traders.DisableFence)
            {
                _fenceOffersUtil.EnableFence();
            }

            if (Config.CurrentConfig.Traders.DisableStartingGifts)
            {
                _giftOffersUtil.EnableGifts();
            }

            _fenceOffersUtil.RefreshFenceOffers();
            _traderOffersUtil.RestoreTraderOffers();
            _fleaMarketOffersUtil.RefreshFleaMarketOffers();

            HardcoreRulesEnabled = false;
            Logger.Info("Disabling hardcore rules...done.");
        }
    }
}
