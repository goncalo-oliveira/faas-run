using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Redpanda.OpenFaaS
{
    /// <summary>
    /// Deals with function execution and response handling
    /// </summary>
    internal class FunctionMiddleware
    {
        private readonly RequestDelegate next;

        public FunctionMiddleware( RequestDelegate nextDelegate )
        {
            next = nextDelegate;
        }

        public async Task InvokeAsync( HttpContext context, IHttpFunction function )
        {
            // execute function
            var result = await function.HandleAsync( context.Request );

            // write result
            var actionContext = new ActionContext( context, context.GetRouteData(), new ActionDescriptor() );

            await result.ExecuteResultAsync( actionContext );
        }
    }
}
