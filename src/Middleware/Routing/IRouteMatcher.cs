using Microsoft.AspNetCore.Routing;

namespace OpenFaaS
{
    internal interface IRouteMatcher
    {
        RouteValueDictionary Match( string routeTemplate, string requestPath );
    }
}
