using HardcoreRules.Helpers;
using SPTarkov.Server.Core.DI;

namespace HardcoreRules.Routers.Internal
{
    public class RouteInfo
    {
        public string Name { get; private set; }
        public IRouteHandler Handler { get; private set; }

        public RouteInfo(string routeName, IRouteHandler routerInstance)
        {
            Name = routeName;
            Handler = routerInstance;
        }

        private string _path = null!;
        public string Path
        {
            get
            {
                if (_path == null)
                {
                    _path = SharedRouterHelpers.GetRoutePath(Name);
                }
                return _path;
            }
        }

        private RouteAction? _action = null;
        public RouteAction? Action
        {
            get
            {
                if (_action == null)
                {
                    _action = CreateRouteAction();
                }

                return _action;
            }
        }

        private RouteAction? CreateRouteAction()
        {
            if (!Handler.ShouldCreateRoutes())
            {
                return null;
            }

            RouteAction routeAction = new RouteAction(Path, async (url, info, sessionId, output) =>
                        await HandleRoute(Name, new RequestData(url, info, sessionId, output)) ?? throw new InvalidOperationException("HandleRoute returned null"));

            return routeAction;
        }

        private ValueTask<string?> HandleRoute(string name, RequestData data)
        {
            if (!Handler.ShouldHandleRoutes())
            {
                return HTTPResponseRepository.NullResponse;
            }

            return Handler.HandleRoute(name, data);
        }
    }
}
