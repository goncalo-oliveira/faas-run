using Microsoft.Extensions.Configuration.EnvironmentVariables;
using OpenFaaS.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration
{
    internal static class SecretsConfigurationExtensions
    {
        public static IConfigurationBuilder AddOpenFaaSSecrets( this IConfigurationBuilder configurationBuilder )
        {
            configurationBuilder.Add( new SecretsConfigurationSource() );

            return ( configurationBuilder );
        }
    }
}
