using SPTarkov.Server.Core.DI;

namespace HardcoreRules.Routers.Internal
{
    public class RouteManager
    {
        private static readonly Dictionary<string, RouteInfo> _allRegisteredRoutes = new();

        private static RouteManager _instance = null!;
        public static RouteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RouteManager();
                }

                return _instance;
            }
        }

        private RouteManager() { }

        public static void RegisterRoutes(IEnumerable<string> routeNames, IRouteHandler handler)
        {
            foreach (string routeName in routeNames)
            {
                RegisterRoute(routeName, handler);
            }
        }

        public static void RegisterRoute(string routeName, IRouteHandler handler)
        {
            if (_allRegisteredRoutes.ContainsKey(routeName))
            {
                throw new InvalidOperationException($"Route \"{routeName}\" is already registered");
            }

            RouteInfo routeInfo = new RouteInfo(routeName, handler);
            _allRegisteredRoutes.Add(routeName, routeInfo);
        }

        public static IEnumerable<RouteAction> GetRoutes(IEnumerable<string> routeNames)
        {
            foreach (string _routeName in routeNames)
            {
                RouteAction? routeAction = GetRoute(_routeName);
                if (routeAction == null)
                {
                    continue;
                }

                yield return routeAction;
            }
        }

        public static RouteAction? GetRoute(string routeName)
        {
            if (!_allRegisteredRoutes.TryGetValue(routeName, out RouteInfo? routeInfo) || routeInfo == null)
            {
                throw new InvalidOperationException($"Cannot retrieve route for \"{routeName}\"");
            }

            return routeInfo.Action;
        }
    }
}
