using HardcoreRules.Helpers;
using HardcoreRules.Services.Internal;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Services
{
    [Injectable(TypePriority = OnLoadOrder.PostSptModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    internal class DebugService : AbstractService
    {
        private DatabaseService _databaseService;
        private TraderOffersUtil _traderOffersUtil;
        private TranslationService _translationService;
        private ToggleHardcoreRulesService _toggleHardcoreRulesService;

        public DebugService
        (
            LoggingUtil logger,
            ConfigUtil config,
            DatabaseService databaseService,
            TraderOffersUtil traderOffersUtil,
            TranslationService translationService,
            ToggleHardcoreRulesService toggleHardcoreRulesService
        ) : base(logger, config)
        {
            _databaseService = databaseService;
            _traderOffersUtil = traderOffersUtil;
            _translationService = translationService;
            _toggleHardcoreRulesService = toggleHardcoreRulesService;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            if (Config.CurrentConfig.IsDebugEnabled())
            {
                ShowTraderIDs();
            }

            if (!DebugHelpers.IsReleaseBuild())
            {
                _toggleHardcoreRulesService.ToggleHardcoreRules(true);
            }
        }

        private void ShowTraderIDs()
        {
            foreach ((MongoId id, Trader trader) in _databaseService.GetTables().Traders)
            {
                if ((trader.Assort?.Items == null) || (trader.Assort.Items.Count == 0))
                {
                    continue;
                }

                bool isWhiteListed = _traderOffersUtil.WhitelistedTraders.Contains(id);
                Logger.Info($"Found trader: {_translationService.GetLocalisedTraderName(trader)} (ID={id}, IsWhitelisted={isWhiteListed})");
            }
        }
    }
}
