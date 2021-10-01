using System;
using System.Diagnostics;
using System.Linq;

namespace OpenFaaS
{
    internal class DockerWrapper
    {
        private readonly string dockerImage = "docker.pkg.github.com/goncalo-oliveira/faas-run/faas-run";

        private string GetImageTag()
        {
            // image tag should match the assembly version
            var imageTag = GetType().Assembly.GetName().Version.ToString( 3 );

// #if DEBUG
//             imageTag = "1.6.1";
//             Console.WriteLine( "WARN: This is a developer build using a fixed Docker 1.6.1 image tag." );
// #endif

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
            var assemblyFullPath = System.IO.Path.GetFullPath( options.Assembly );
            var assemblyPath = System.IO.Path.GetDirectoryName( assemblyFullPath );
            var assemblyFile = System.IO.Path.GetFileName( assemblyFullPath );

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
                !string.IsNullOrEmpty( options.ContainerName )
                    ? $"--name faas_{options.ContainerName}"
                    : string.Empty,
                $"-p {options.Port}:80",
                $"-v {assemblyPath}:/home/app/:ro",
                !string.IsNullOrEmpty( configPath )
                    ? $"-v {configPath}:/home/config.json:ro"
                    : string.Empty,
                $"{dockerImage}:{imageTag}",
                "faas-run",
                $"/home/app/{assemblyFile}",
                "-p 80",
                options.NoAuth ? "--no-auth" : string.Empty,
                !string.IsNullOrEmpty( configPath )
                    ? "--config /home/config.json"
                    : string.Empty,
            }
            .Where( x => !string.IsNullOrEmpty( x ) )
            .ToArray();

            var dockerArgs = string.Join( (char)0x20, args );

            //  Console.WriteLine( $"docker pull {dockerImage}:{imageTag}" );
            //  Console.WriteLine( "docker " + dockerArgs );
            //  return;

            // pull faas-run image
            var pullExitCode = Exec( "docker", $"pull {dockerImage}:{imageTag}" );

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
            Exec( "docker", $"ps --filter ancestor={dockerImage}:{imageTag}" );
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
