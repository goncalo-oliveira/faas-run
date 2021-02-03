using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OpenFaaS.Functions.Middleware
{
    /// <summary>
    /// Deals with authentication
    /// </summary>
    internal class AuthenticationMiddleware
    {
        private readonly HttpRequestHandlerOptions options;
        private readonly RequestDelegate next;

        public AuthenticationMiddleware( RequestDelegate nextDelegate
            , IOptions<HttpRequestHandlerOptions> handlerOptionsAccessor )
        {
            next = nextDelegate;
            options = handlerOptionsAccessor.Value;
        }

        public async Task InvokeAsync( HttpContext context, HttpAuthenticationHandler authHandler )
        {
            if ( !options.SkipAuth )
            {
                await authHandler.AuthenticateAsync( context );
            }

            await next( context );
        }
    }
}
