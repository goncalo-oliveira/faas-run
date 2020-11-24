using Microsoft.AspNetCore.Routing;

namespace Redpanda.OpenFaaS
{
    internal interface IRouteMatcher
    {
        RouteValueDictionary Match( string routeTemplate, string requestPath );
    }
}
