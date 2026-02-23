using HardcoreRules.Helpers;
using HardcoreRules.Services;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.OfferSourceUtils
{
    [Injectable(InjectionType.Singleton)]
    public class FleaMarketOffersUtil
    {
        private FleaMarketOfferSource FleaMarket;

        private LoggingUtil _loggingUtil;
        private ConfigUtil _configUtil;
        private ConfigServer _configServer;
        private DatabaseService _databaseService;
        private OfferModificationUtil _offerModificationUtil;
        private RagfairOfferGenerator _ragfairOfferGenerator;
        private RagfairOfferService _ragfairOfferService;
        private RagfairController _ragfairController;

        private IEnumerable<Trader> _tradersWithOffersNotIncludingFence => _databaseService.GetTables().Traders.Values
                .NotIncludingFence()
                .WithOffers();

        public FleaMarketOffersUtil
        (
            LoggingUtil loggingUtil,
            ConfigUtil configUtil,
            ConfigServer configServer,
            DatabaseService databaseService,
            OfferModificationUtil offerModificationUtil,
            RagfairOfferGenerator ragfairOfferGenerator,
            RagfairOfferService ragfairOfferService,
            RagfairController ragfairController
        )
        {
            _loggingUtil = loggingUtil;
            _configUtil = configUtil;
            _configServer = configServer;
            _databaseService = databaseService;
            _offerModificationUtil = offerModificationUtil;
            _ragfairOfferGenerator = ragfairOfferGenerator;
            _ragfairOfferService = ragfairOfferService;
            _ragfairController = ragfairController;

            FleaMarket = new FleaMarketOfferSource(_loggingUtil, _configServer, _databaseService, _ragfairOfferGenerator);
        }

        public void DisableFleaMarket() => FleaMarket.Disable();
        public void EnableFleaMarket() => FleaMarket.Enable();
        public void RefreshFleaMarketOffers()
        {
            FleaMarket.Refresh();

            if (ToggleHardcoreRulesService.HardcoreRulesEnabled)
            {
                RemoveBannedFleaMarketOffers();
            }
        }

        public GetOffersResult GetFleaMarketOffers(MongoId sessionId, SearchRequestData searchRequestData) => _ragfairController.GetOffers(sessionId, searchRequestData);

        public void RemoveBannedFleaMarketOffers()
        {
            bool onlyBarterOffers = _configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers;
            _loggingUtil.Info($"Removing all {(onlyBarterOffers ? "" : "cash ")}flea market offers...");

            List<RagfairOffer> offers = _ragfairOfferService.GetOffers();
            foreach (RagfairOffer offer in offers)
            {
                if (!ShouldRemoveFleaMarketOffer(offer))
                {
                    continue;
                }

                _ragfairOfferService.RemoveOfferById(offer.Id);
            }

            CreateFleaMarketOffersForTraders();
        }

        public void CreateFleaMarketOffersForTraders()
        {
            _loggingUtil.Info("Adding flea market offers for traders...");

            foreach (Trader trader in _tradersWithOffersNotIncludingFence)
            {
                _ragfairOfferGenerator.GenerateFleaOffersForTrader(trader.Base.Id);
            }
        }

        public bool ShouldRemoveFleaMarketOffer(RagfairOffer offer)
        {
            if (!_configUtil.CurrentConfig.Services.FleaMarket.Enabled)
            {
                return true;
            }

            if (_configUtil.CurrentConfig.Services.FleaMarket.OnlyBarterOffers && !_offerModificationUtil.IsABarterOffer(offer))
            {
                return true;
            }

            return false;
        }
    }
}
