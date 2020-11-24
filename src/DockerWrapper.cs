using System;
using System.Diagnostics;
using System.Linq;

namespace Redpanda.OpenFaaS
{
    internal class DockerWrapper
    {
        private string GetImageTag()
        {
            // image tag should match the assembly version
            var imageTag = GetType().Assembly.GetName().Version.ToString( 3 );

            if ( imageTag.EndsWith( ".0" ) )
            {
                // trim revision number if zero
                return imageTag.Substring( 0, imageTag.Length - 2 );
            }

            return ( imageTag );
        }

        public void RunDetached( Options options )
        {
            // to run detached we need the full path, not the relative
            var assemblyPath = System.IO.Path.GetFullPath( options.Assembly );
            var assemblyFile = System.IO.Path.GetFileName( assemblyPath );

            var configPath = !string.IsNullOrEmpty( options.Config ) && System.IO.File.Exists( options.Config )
                ? System.IO.Path.GetFullPath( options.Config )
                : string.Empty;

            if ( !string.IsNullOrEmpty( configPath ) )
            {
                Console.WriteLine( $"Using '{options.Config}' configuration file." );
            }

            var imageTag = GetImageTag();

            var args = new string[]
            {
                "run",
                "-d",
                $"-p {options.Port}:80",
                $"-v {assemblyPath}:/home/app/{assemblyFile}:ro",
                !string.IsNullOrEmpty( configPath )
                    ? $"-v {configPath}:/home/app/config.json:ro"
                    : string.Empty,
                $"redpandaltd/faas-run:{imageTag}",
                "faas-run",
                $"/home/app/{assemblyFile}",
                "-p 80",
                options.NoAuth ? "--no-auth" : string.Empty,
                !string.IsNullOrEmpty( configPath )
                    ? $"/home/app/config.json"
                    : string.Empty,
            }
            .Where( x => !string.IsNullOrEmpty( x ) )
            .ToArray();

            var dockerArgs = string.Join( (char)0x20, args );

            // Console.WriteLine( $"docker pull redpandaltd/faas-run:{imageTag}" );
            // Console.WriteLine( "docker " + dockerArgs );

            // pull faas-run image
            var pullExitCode = Exec( "docker", $"pull redpandaltd/faas-run:{imageTag}" );

            if ( pullExitCode > 0 )
            {
                // failed to pull image
                return;
            }

            // run function with Docker
            var runExitCode = Exec( "docker", dockerArgs );

            if ( pullExitCode > 0 )
            {
                return;
            }

            // display container info
            Exec( "docker", $"ps --filter ancestor=redpandaltd/faas-run:{imageTag}" );
        }

        private int Exec( string fileName, string args = null )
        {
           
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true                    
                }
            };

            process.Start();
            process.WaitForExit();

            return ( process.ExitCode );
        }
    }
}
