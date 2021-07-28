using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OpenFaaS.Functions
{
    public class Startup : IPluginStartup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddMvc();

            services.AddSingleton<ISystemClock, SystemClock>();

            // add root request handler to the container
            services.AddSingleton<Middleware.IRouteMatcher, Middleware.RouteMatcher>();
            services.TryAddScoped<Middleware.HttpAuthenticationHandler>();
            services.Configure<HttpRequestHandlerOptions>( options =>
            {
                options.SkipAuth = Configuration.GetValue<bool>( "Args:SkipAuth" );

                if ( options.SkipAuth )
                {
                    Console.WriteLine( "Skipping authentication and authorization" );
                }
            } );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            app.UseDeveloperExceptionPage();

            Console.WriteLine();
            Console.WriteLine( "Running..." );

            app.UseMiddleware<Middleware.RoutingMiddleware>()
                .UseMiddleware<Middleware.AuthenticationMiddleware>()
                .UseMiddleware<Middleware.AuthorizationMiddleware>()
                .UseMiddleware<Middleware.FunctionMiddleware>();
        }
    }
}
