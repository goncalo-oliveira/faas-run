using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Redpanda.OpenFaaS
{
    internal static class FunctionServiceExtensions
    {
        private static readonly string typeName = "OpenFaaS.Startup";
        
        public static void AddFunctionServices( this IServiceCollection services, IConfiguration configuration )
        {
            var type = AssemblyResolver.Default.Assembly.GetType( typeName);

            if ( type == null )
            {
                throw new TypeLoadException( $"Failed to load '{typeName}' type." );
            }

            var instance = AssemblyResolver.Default.Assembly.CreateInstance( typeName, false, BindingFlags.CreateInstance, null, new object[]
            {
                configuration
            }, null, null );

            type.InvokeMember( "ConfigureServices", BindingFlags.InvokeMethod, null, instance, new object[]
            { 
                services 
            } );
        }
    }
}
