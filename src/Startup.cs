using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenFaaS
{
    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private Action<IApplicationBuilder, IWebHostEnvironment> configure;

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddCors( options =>
            {
                options.AddPolicy( "AllowAll", p => p
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader() );
            } );

            // initializes plugin type
            services.AddPlugin( Configuration )
                .AddRouting( options => services.ConfigurePluginRouting( options ) )
                .ConfigurePluginServices();

            bool isHttpFunction = services.Any( x => x.ServiceType == typeof( IHttpFunction ) );

            IPluginStartup pluginStartup = isHttpFunction
                ? new Functions.Startup( Configuration )
                : new Api.Startup( Configuration );

            pluginStartup.ConfigureServices( services );

            configure = pluginStartup.Configure;
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            configure?.Invoke( app, env );
        }
    }
}
