using DansDevTools.Utils;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace DansDevTools.Routers.Internal;

public abstract class AbstractDynamicRouter : DynamicRouter, IRouteHandler
{
    protected static ConfigUtil Config { get; private set; } = null!;

    protected LoggingUtil Logger { get; private set; } = null!;
    protected JsonUtil JsonUtil { get; private set; } = null!;

    public AbstractDynamicRouter(IEnumerable<string> _routeNames, LoggingUtil logger, ConfigUtil config, JsonUtil jsonUtil) : base(jsonUtil, RouteManager.GetRoutes(_routeNames))
    {
        if (Config == null)
        {
            Config = config;
        }

        Logger = logger;
        JsonUtil = jsonUtil;

        RouteManager.RegisterRoutes(_routeNames, this);
    }

    public virtual bool ShouldCreateRoutes() => Config.IsModEnabled;
    public virtual bool ShouldHandleRoutes() => true;

    public abstract ValueTask<string?> HandleRoute(string routeName, RequestData routerData);
}