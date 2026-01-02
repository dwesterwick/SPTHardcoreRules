using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostSptModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    public class ToggleHardcoreRulesService : AbstractService
    {
        public bool HardcoreRulesEnabled { get; private set; } = false;

        private TraderOffersUtil _traderOffersUtil;

        public ToggleHardcoreRulesService
        (
            LoggingUtil logger,
            ConfigUtil config,
            TraderOffersUtil traderOffersUtil
        ) : base(logger, config)
        {
            _traderOffersUtil = traderOffersUtil;
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
                _traderOffersUtil.EnableFleaMarket();
            }

            if (Config.CurrentConfig.Traders.DisableFence)
            {
                _traderOffersUtil.EnableFence();
            }

            if (Config.CurrentConfig.Traders.DisableStartingGifts)
            {
                _traderOffersUtil.EnableGifts();
            }

            HardcoreRulesEnabled = true;
            Logger.Info("Enabling hardcore rules...done.");
        }

        private void DisableHardcoreRules()
        {
            Logger.Info("Disabling hardcore rules...");

            if (!Config.CurrentConfig.Services.FleaMarket.Enabled)
            {
                _traderOffersUtil.DisableFleaMarket();
            }

            if (Config.CurrentConfig.Traders.DisableFence)
            {
                _traderOffersUtil.DisableFence();
            }

            if (Config.CurrentConfig.Traders.DisableStartingGifts)
            {
                _traderOffersUtil.DisableGifts();
            }

            HardcoreRulesEnabled = false;
            Logger.Info("Disabling hardcore rules...done.");
        }
    }
}
