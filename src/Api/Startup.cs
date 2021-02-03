using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace OpenFaaS.Api
{
    public class Startup : IPluginStartup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices( IServiceCollection services )
        {
            var mvcBuilder = services.AddControllers( options =>
            {
            } );

            mvcBuilder.AddNewtonsoftJson( options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.Converters.Add( new StringEnumConverter( new CamelCaseNamingStrategy(), false ) );
            });
            // Replaced with Newtonsoft because Microsoft's serializer doesn't do polymorphic serialization

            mvcBuilder.AddPluginControllers();
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            if ( !Configuration.GetValue<bool>( "Args:SkipAuth" ) )
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // allow api implementation to customize the pipeline
            app.ConfigurePlugin( env );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
