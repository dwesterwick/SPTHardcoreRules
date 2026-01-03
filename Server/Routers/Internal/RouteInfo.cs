using SPTarkov.Server.Core.Models.Eft.Common;

namespace HardcoreRules.Routers.Internal
{
    public class RouteInfo : TypedRouteInfo<EmptyRequestData>
    {
        public RouteInfo(string routeName, IRouteHandler routerInstance) : base(routeName, routerInstance)
        {

        }
    }
}