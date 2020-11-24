using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Redpanda.OpenFaaS
{
    /// <summary>
    /// Deals with authorization
    /// </summary>
    internal class AuthorizationMiddleware
    {
        private readonly HttpRequestHandlerOptions options;
        private readonly RequestDelegate next;

        public AuthorizationMiddleware( RequestDelegate nextDelegate
            , IOptions<HttpRequestHandlerOptions> handlerOptionsAccessor )
        {
            next = nextDelegate;
            options = handlerOptionsAccessor.Value;
        }

        public Task InvokeAsync( HttpContext context, IHttpFunction function )
        {
            if ( !options.SkipAuth )
            {
                // enforce authorization when required
                var authAttributes = function.GetAuthorizeAttributes();

                if ( function.GetAuthorizeAttributes().Any() && !context.User.Identity.IsAuthenticated )
                {
                    context.Response.StatusCode = 401;

                    return context.Response.WriteAsync( "Unauthorized" );
                }
            }

            return next( context );
        }
    }
}
