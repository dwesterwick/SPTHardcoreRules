using HardcoreRules.Helpers;
using HardcoreRules.Routers.Internal;
using HardcoreRules.Services;
using HardcoreRules.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    public class RefreshRagfairRouter : AbstractStaticRouter
    {
        private static readonly string[] _routeNames = ["/client/ragfair/find"];

        private HttpResponseUtil _httpResponseUtil;
        private ToggleHardcoreRulesService _toggleHardcoreRulesService;
        private TraderOffersUtil _traderOffersUtil;
        private RagfairController _ragfairController;

        public RefreshRagfairRouter
        (
            LoggingUtil logger,
            ConfigUtil config,
            JsonUtil jsonUtil,
            HttpResponseUtil httpResponseUtil,
            ToggleHardcoreRulesService toggleHardcoreRulesService,
            TraderOffersUtil traderOffersUtil,
            RagfairController ragfairController
        ) : base(_routeNames, logger, config, jsonUtil)
        {
            _httpResponseUtil = httpResponseUtil;
            _toggleHardcoreRulesService = toggleHardcoreRulesService;
            _traderOffersUtil = traderOffersUtil;
            _ragfairController = ragfairController;
        }

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            if (!MustUpdateOffers() || !HasCashOffers(routerData.SessionId, routerData.Info))
            {
                return new ValueTask<string?>(routerData.Output);
            }

            Logger.Info("Found cash offers in flea market search result. Refeshing offers...");
            RefreshFleaMarketOffers();

            GetOffersResult offers = GetFleaMarketOffers(routerData.SessionId, routerData.Info);

            string jsonResponse = _httpResponseUtil.GetBody(offers);
            return new ValueTask<string?>(jsonResponse);
        }

        private void RefreshFleaMarketOffers()
        {
            _traderOffersUtil.RefreshFleaMarketOffers();

            if (_toggleHardcoreRulesService.HardcoreRulesEnabled)
            {
                _traderOffersUtil.RemoveBannedFleaMarketOffers();
            }
        }

        private bool MustUpdateOffers()
        {
            if (!Config.CurrentConfig.IsModEnabled())
            {
                return false;
            }

            if (!_toggleHardcoreRulesService.HardcoreRulesEnabled)
            {
                return false;
            }

            if (!Config.CurrentConfig.Services.FleaMarket.OnlyBarterOffers)
            {
                return false;
            }

            return true;
        }

        private bool HasCashOffers(MongoId sessionId, IRequestData info)
        {
            GetOffersResult offers = GetFleaMarketOffers(sessionId, info);
            return _traderOffersUtil.HasCashOffers(offers);
        }

        private GetOffersResult GetFleaMarketOffers(MongoId sessionId, IRequestData info)
        {
            SearchRequestData? searchRequestData = info as SearchRequestData;
            if (searchRequestData == null)
            {
                throw new InvalidCastException("Server request data is not SearchRequestData");
            }

            return _ragfairController.GetOffers(sessionId, searchRequestData);
        }
    }
}
