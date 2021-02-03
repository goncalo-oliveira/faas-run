using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenFaaS
{
    internal static class PluginServiceExtensions
    {
        private static readonly string typeName = "OpenFaaS.Startup";

        private static object startup;
        
        public static void AddPluginControllers( this IMvcBuilder builder )
        {
            builder.AddApplicationPart( AssemblyResolver.Default.Assembly );
        }

        public static IServiceCollection ConfigurePluginServices( this IServiceCollection services, IConfiguration configuration )
        {
            startup = AssemblyResolver.Default.Assembly.CreateInstance( typeName, false, BindingFlags.CreateInstance, null, new object[]
            {
                configuration
            }, null, null );

            startup.GetType().InvokeMember( "ConfigureServices", BindingFlags.InvokeMethod, null, startup, new object[]
            { 
                services 
            } );

            return ( services );
        }

        public static IApplicationBuilder ConfigurePlugin( this IApplicationBuilder app, IWebHostEnvironment env )
        {
            var type = startup.GetType();

            var configureMethod = type.GetMethod( "Configure", BindingFlags.Public | BindingFlags.Instance, null, new Type[]
            {
                typeof( IApplicationBuilder ), typeof( bool )
            }, null );

            // the Configure( IApplicationBuilder, bool ) method is optional

            configureMethod?.Invoke( startup, new object[]
            { 
                app, env.IsDevelopment()
            } );

            return ( app );
        }
    }
}
