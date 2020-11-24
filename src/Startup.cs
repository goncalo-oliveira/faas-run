using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;

namespace Redpanda.OpenFaaS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors( options =>
            {
                options.AddPolicy( "AllowAll", p => p
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader() );
            } );

            services.AddRouting( options =>
            {
                options.LowercaseUrls = true;
            } );

            services.AddMvc()
                .AddNewtonsoftJson( options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                } );
            // Replaced with Newtonsoft because Microsoft's serializer doesn't do polymorphic serialization

            services.AddSingleton<ISystemClock, SystemClock>();

            // allow function implementation to add services to the container
            services.AddFunctionServices( Configuration );

            // add root request handler to the container
            services.AddSingleton<IRouteMatcher, RouteMatcher>();
            services.TryAddScoped<HttpAuthenticationHandler>();
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            Console.WriteLine();
            Console.WriteLine( "Running..." );

            app.UseMiddleware<RoutingMiddleware>()
                .UseMiddleware<AuthenticationMiddleware>()
                .UseMiddleware<AuthorizationMiddleware>()
                .UseMiddleware<FunctionMiddleware>();
        }
    }
}
