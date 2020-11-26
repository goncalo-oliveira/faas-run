using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Redpanda.OpenFaaS
{
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly ICompilationAssemblyResolver resolver;
        private readonly DependencyContext dependencyContext;
        private readonly AssemblyLoadContext loadContext;

        private readonly string packagesPath;

        public AssemblyResolver( string path )
        {
            packagesPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), ".nuget/packages/" );

            // load main assembly
            var assemblyPath = Path.GetFullPath( path );
            Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath( assemblyPath );

            Console.WriteLine( $"Loaded function assembly {path}." );

            dependencyContext = DependencyContext.Load( Assembly );
            resolver = new CompositeCompilationAssemblyResolver(
                new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver( Path.GetDirectoryName( assemblyPath ) ),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver()
                } );

            loadContext = AssemblyLoadContext.GetLoadContext( Assembly );

            loadContext.Resolving += OnResolving;

            if ( Default == null )
            {
                Default = this;
            }
        }

        public Assembly Assembly { get; }

        public static AssemblyResolver Default { get; private set; }

        private Assembly OnResolving( AssemblyLoadContext context, AssemblyName name )
        {
            var library = dependencyContext.RuntimeLibraries
                .Where( runtime => string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase) )
                .FirstOrDefault();

            if ( library == null )
            {
                library = dependencyContext.RuntimeLibraries.Where( runtime => 
                {
                    var path = runtime.RuntimeAssemblyGroups.SelectMany( group => group.AssetPaths ).FirstOrDefault();

                    return path?.EndsWith( $"{name.Name}.dll" ) == true;
                } )
                .FirstOrDefault();
            }

            if ( library != null )
            {
                var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable );

                var assemblies = new List<string>();
                resolver.TryResolveAssemblyPaths( wrapper, assemblies );

                if (assemblies.Count > 0)
                {
                    var dependency = loadContext.LoadFromAssemblyPath(assemblies[0]);

                    Console.WriteLine( $"Loaded dependency {assemblies[0]}." );

                    return ( dependency );
                }
            }

            // load 'manually'
            var runtimePath = Path.Combine( packagesPath, $"{library.Name.ToLower()}/{library.Version}/" );
            var path = string.Concat( runtimePath, library.RuntimeAssemblyGroups.First().AssetPaths.First() );

            try
            {
                var dependency = loadContext.LoadFromAssemblyPath( path );

                Console.WriteLine( $"Loaded dependency {Path.GetFileName( path )}." );

                return ( dependency );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );

                var loadedAssembly = context.Assemblies.SingleOrDefault( x => x.GetName().Name.Equals( library.Name ) );

                if ( loadedAssembly != null )
                {
                    return ( loadedAssembly );
                }
            }

            return ( null );
        }

        public void Dispose()
        {
            loadContext.Resolving -= OnResolving;
        }
    }
}