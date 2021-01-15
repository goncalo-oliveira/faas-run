using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace OpenFaaS.Configuration
{
    internal class SecretsConfigurationProvider : ConfigurationProvider
    {
        private readonly string secretsPath = "/var/openfaas/secrets/";

        public override void Load()
        {
            if ( !Directory.Exists( secretsPath ) )
            {
                return;
            }

            try
            {
                var secrets = Directory.GetFiles( secretsPath );

                foreach ( var secret in secrets )
                {
                    var secretName = string.Concat( "_secret_", Path.GetFileName( secret ) );
                    var secretValue = File.ReadAllBytes( secret );

                    Data.Add( secretName, Convert.ToBase64String( secretValue ) );
                }
            }
            catch ( Exception )
            { }
        }
    }
}
