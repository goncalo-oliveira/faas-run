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

        public static IServiceCollection AddPlugin( this IServiceCollection services, IConfiguration configuration )
        {
            startup = AssemblyResolver.Default.Assembly.CreateInstance( typeName, false, BindingFlags.CreateInstance, null, new object[]
            {
                configuration
            }, null, null );

            return ( services );
        }
        
        public static IMvcBuilder AddPluginControllers( this IMvcBuilder builder )
        {
            return builder.AddApplicationPart( AssemblyResolver.Default.Assembly );
        }

        public static IServiceCollection ConfigurePluginRouting( this IServiceCollection services, Microsoft.AspNetCore.Routing.RouteOptions options )
        {
            var configureRoutingMethod = startup.GetType().GetMethod( "ConfigureRouting", BindingFlags.Public | BindingFlags.Instance, null, new Type[]
            {
                typeof( Microsoft.AspNetCore.Routing.RouteOptions )
            }, null );

            // the ConfigureRouting( RoutingOptions ) method is optional
            configureRoutingMethod?.Invoke( startup, new object[]
            { 
                options
            } );

            return ( services );
        }

        public static IServiceCollection ConfigurePluginMvc( this IServiceCollection services, Microsoft.AspNetCore.Mvc.MvcOptions options )
        {
            var configureMvcMethod = startup.GetType().GetMethod( "ConfigureMvc", BindingFlags.Public | BindingFlags.Instance, null, new Type[]
            {
                typeof( Microsoft.AspNetCore.Mvc.MvcOptions )
            }, null );

            // the ConfigureMvc( MvcOptions ) method is optional
            configureMvcMethod?.Invoke( startup, new object[]
            { 
                options
            } );

            return ( services );
        }

        public static IServiceCollection ConfigurePluginServices( this IServiceCollection services )
        {
            startup.GetType().InvokeMember( "ConfigureServices", BindingFlags.InvokeMethod, null, startup, new object[]
            { 
                services 
            } );

            return ( services );
        }

        public static IApplicationBuilder ConfigurePlugin( this IApplicationBuilder app, IWebHostEnvironment env )
        {
            var configureMethod = startup.GetType().GetMethod( "Configure", BindingFlags.Public | BindingFlags.Instance, null, new Type[]
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
