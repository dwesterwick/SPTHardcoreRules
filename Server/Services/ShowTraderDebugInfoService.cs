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
    [Injectable(TypePriority = OnLoadOrder.TraderRegistration + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
    public class ShowTraderDebugInfoService : AbstractService
    {
        private DatabaseService _databaseService;
        private TraderOffersUtil _traderOffersUtil;
        private TranslationService _translationService;

        public ShowTraderDebugInfoService
        (
            LoggingUtil logger,
            ConfigUtil config,
            DatabaseService databaseService,
            TraderOffersUtil traderOffersUtil,
            TranslationService translationService
        ) : base(logger, config)
        {
            _databaseService = databaseService;
            _traderOffersUtil = traderOffersUtil;
            _translationService = translationService;
        }

        protected override void OnLoadIfModIsEnabled()
        {
            if (Config.CurrentConfig.IsDebugEnabled())
            {
                ShowTraderIDs();
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
