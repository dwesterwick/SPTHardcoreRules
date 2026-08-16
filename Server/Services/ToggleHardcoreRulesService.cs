using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.Preload + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    internal class ToggleHardcoreRulesService : AbstractService
    {
        public static bool HardcoreRulesEnabled { get; private set; } = false;

        private RagfairConfig _ragfairConfig;

        private TraderOffersUtil _traderOffersUtil;
        private GiftOffersUtil _giftOffersUtil;
        private FenceOffersUtil _fenceOffersUtil;
        private FleaMarketOffersUtil _fleaMarketOffersUtil;

        private ObjectCache<double> _fleaMarketPlayerBarterOfferChance = new();

        public ToggleHardcoreRulesService
        (
            LoggingUtil logger,
            ConfigUtil config,
            RagfairConfig ragfairConfig,
            TraderOffersUtil traderOffersUtil,
            GiftOffersUtil giftOffersUtil,
            FenceOffersUtil fenceOffersUtil,
            FleaMarketOffersUtil fleaMarketOffersUtil
        ) : base(logger, config)
        {
            _ragfairConfig = ragfairConfig;
            _traderOffersUtil = traderOffersUtil;
            _giftOffersUtil = giftOffersUtil;
            _fenceOffersUtil = fenceOffersUtil;
            _fleaMarketOffersUtil = fleaMarketOffersUtil;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            _fleaMarketPlayerBarterOfferChance.CacheValueAndThrowIfNull(_ragfairConfig.Dynamic.Barter.ChancePercent);
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
            else
            {
                _ragfairConfig.Dynamic.Barter.ChancePercent = Config.CurrentConfig.Services.FleaMarket.BarterOfferChanceForPlayers;
            }

            if (Config.CurrentConfig.Traders.DisableFence)
            {
                _fenceOffersUtil.DisableFence();
            }

            if (Config.CurrentConfig.Traders.DisableStartingGifts)
            {
                _giftOffersUtil.DisableGifts();
            }

            _traderOffersUtil.RemoveBannedTraderOffers();
            
            HardcoreRulesEnabled = true;

            _fenceOffersUtil.RefreshFenceOffers();
            _fleaMarketOffersUtil.RefreshFleaMarketOffers();

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

            _traderOffersUtil.RestoreTraderOffers();
            _ragfairConfig.Dynamic.Barter.ChancePercent = _fleaMarketPlayerBarterOfferChance.GetValueAndThrowIfNull();

            HardcoreRulesEnabled = false;

            _fenceOffersUtil.RefreshFenceOffers();
            _fleaMarketOffersUtil.RefreshFleaMarketOffers();

            Logger.Info("Disabling hardcore rules...done.");
        }
    }
}
