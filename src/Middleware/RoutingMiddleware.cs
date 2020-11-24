using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Redpanda.OpenFaaS
{
    /// <summary>
    /// Deals with routing and routing templates
    /// </summary>
    internal class RoutingMiddleware
    {
        private readonly ILogger log;
        private readonly HttpFunctionOptions options;
        private readonly IRouteMatcher routeMatcher;
        private readonly RequestDelegate next;

        public RoutingMiddleware( RequestDelegate nextDelegate
            , ILoggerFactory loggerFactory
            , IOptions<HttpFunctionOptions> optionsAccessor
            , IRouteMatcher internalRouteMatcher )
        {
            next = nextDelegate;
            log = loggerFactory.CreateLogger<RoutingMiddleware>();
            options = optionsAccessor.Value;
            routeMatcher = internalRouteMatcher;
        }

        public Task InvokeAsync( HttpContext context, IHttpFunction function )
        {
            var httpAttributes = function.GetHttpMethodAttributes();

            if ( httpAttributes.Any() )
            {
                var httpAttribute = httpAttributes.GetMethod( context.Request.Method );

                if ( httpAttribute == null )
                {
                    // method not allowed
                    context.Response.StatusCode = 405;

                    return context.Response.WriteAsync( "Method Not Allowed" );
                }

                if ( !string.IsNullOrEmpty( httpAttribute.Template ) )
                {
                    // match route template
                    if ( !httpAttribute.MatchRouteTemplate( context, routeMatcher ) )
                    {
                        // path doesn't match route template
                        return WriteNotFoundAsync( context );
                    }
                }
                else if ( !options.IgnoreRoutingRules && ( context.Request.Path != "/" ) )
                {
                    // attribute template is null. reject custom path unless allowed by options
                    return WriteNotFoundAsync( context );
                }
            }
            else if ( !options.IgnoreRoutingRules && ( context.Request.Path != "/" ) )
            {
                // if there are no http modifiers
                // we reject a custom path unless overriden by options
                return WriteNotFoundAsync( context );
            }

            return next( context );
        }

        private Task WriteNotFoundAsync( HttpContext context )
        {
            log.LogInformation( $"{context.Request.Method} {context.Request.Path}  404" );

            context.Response.StatusCode = 404;

            return context.Response.WriteAsync( "Not Found" );
        }
    }
}
