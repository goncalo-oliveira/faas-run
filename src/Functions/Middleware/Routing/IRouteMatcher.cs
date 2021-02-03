using Microsoft.AspNetCore.Routing;

namespace OpenFaaS.Functions.Middleware
{
    internal interface IRouteMatcher
    {
        RouteValueDictionary Match( string routeTemplate, string requestPath );
    }
}
