using System;
using System.Collections.Generic;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Redpanda.OpenFaaS
{
    class Options
    {
        [Option( 'p', "port", Default = 9000, HelpText = "Listening port")]
        public int Port { get; set; }

        [Option( "config", Default = "config.json", HelpText = "Configuration file")]
        public string Config { get; set; }

        [Option( "no-auth", HelpText = "Skip authentication/authorization")]
        public bool NoAuth { get; set; }

        [Option( 'd', "detach", HelpText = "Run function in background (Docker)" )]
        public bool Detach { get; set; }

        [Value( 0, Required = true, MetaName = "assembly", HelpText = "Function assembly path" )]
        public string Assembly { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var parsed = CommandLine.Parser.Default.ParseArguments<Options>( args );

            var processId = System.Diagnostics.Process.GetCurrentProcess().Id;

            Console.WriteLine("OpenFaaS ASPNET Function Loader");
            Console.WriteLine();

            parsed.WithParsed( options =>
            {
                if ( options.Detach )
                {
                    new DockerWrapper()
                        .RunDetached( options );

                    return;
                }

                Console.WriteLine( $"To debug attach to process id {processId}." );
                Console.WriteLine();

                var resolver = new AssemblyResolver( options.Assembly );

                CreateHostBuilder( args, options ).Build().Run();    
            } );
        }

        public static IHostBuilder CreateHostBuilder( string[] args, Options options ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureLogging( logging =>
                {
                    logging.AddFilter( "Microsoft.AspNetCore.DataProtection", LogLevel.Warning )
                        .AddFilter( "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor", LogLevel.Warning );
                } )
                .ConfigureWebHostDefaults( webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration( configBuilder =>
                    {
                        if ( System.IO.File.Exists( options.Config ) )
                        {
                            Console.WriteLine( $"Using '{options.Config}' configuration file." );
                        }

                        configBuilder.SetBasePath( Environment.CurrentDirectory );
                        configBuilder.AddJsonFile( options.Config, optional: true, reloadOnChange: false );
                        configBuilder.AddEnvironmentVariables();
                        configBuilder.AddOpenFaaSSecrets();
                        configBuilder.AddInMemoryCollection( new Dictionary<string, string>
                        {
                            { "Args:SkipAuth", options.NoAuth.ToString() }
                        } );
                    } );

                    webBuilder.UseKestrel()
                        .UseStartup<Startup>();

                    webBuilder.UseUrls( $"http://*:{options.Port}" );
                } );
    }
}
