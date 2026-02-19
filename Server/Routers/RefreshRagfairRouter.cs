using HardcoreRules.Helpers;
using HardcoreRules.Routers.Internal;
using HardcoreRules.Services;
using HardcoreRules.Utils;
using HardcoreRules.Utils.OfferSourceUtils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace HardcoreRules.Routers
{
    [Injectable]
    internal class RefreshRagfairRouter : AbstractTypedStaticRouter<SearchRequestData>
    {
        private static readonly string[] _routeNames = ["/client/ragfair/find"];

        private HttpResponseUtil _httpResponseUtil;
        private OfferModificationUtil _traderOfferModificationUtil;
        private FleaMarketOffersUtil _fleaMarketOffersUtil;

        public RefreshRagfairRouter
        (
            LoggingUtil logger,
            ConfigUtil config,
            JsonUtil jsonUtil,
            HttpResponseUtil httpResponseUtil,
            OfferModificationUtil traderOffersUtil,
            FleaMarketOffersUtil fleaMarketOffersUtil
        ) : base(_routeNames, logger, config, jsonUtil)
        {
            _httpResponseUtil = httpResponseUtil;
            _traderOfferModificationUtil = traderOffersUtil;
            _fleaMarketOffersUtil = fleaMarketOffersUtil;
        }

        public override ValueTask<string?> HandleRoute(string routeName, RequestData routerData)
        {
            if (!MustUpdateFleaMarketOffers() || !HasCashFleaMarketOffers(routerData.SessionId, routerData.Info))
            {
                return new ValueTask<string?>(routerData.Output);
            }

            Logger.Info("Found cash offers in flea market search result. Refeshing offers...");
            _fleaMarketOffersUtil.RefreshFleaMarketOffers();

            GetOffersResult offers = GetFleaMarketOffers(routerData.SessionId, routerData.Info);

            string jsonResponse = _httpResponseUtil.GetBody(offers);
            return new ValueTask<string?>(jsonResponse);
        }

        private bool MustUpdateFleaMarketOffers()
        {
            if (!Config.CurrentConfig.IsModEnabled())
            {
                return false;
            }

            if (!ToggleHardcoreRulesService.HardcoreRulesEnabled)
            {
                return false;
            }

            if (!Config.CurrentConfig.Services.FleaMarket.OnlyBarterOffers)
            {
                return false;
            }

            return true;
        }

        private bool HasCashFleaMarketOffers(MongoId sessionId, IRequestData info)
        {
            GetOffersResult offers = GetFleaMarketOffers(sessionId, info);
            return _traderOfferModificationUtil.HasCashOffers(offers);
        }

        private GetOffersResult GetFleaMarketOffers(MongoId sessionId, IRequestData info)
        {
            SearchRequestData? searchRequestData = info as SearchRequestData;
            if (searchRequestData == null)
            {
                throw new InvalidCastException($"Server request data is not SearchRequestData. Type={info.GetType()}");
            }

            return _fleaMarketOffersUtil.GetFleaMarketOffers(sessionId, searchRequestData);
        }
    }
}
