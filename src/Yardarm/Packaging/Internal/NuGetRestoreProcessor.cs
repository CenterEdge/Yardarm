using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Packaging.Signing;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Yardarm.Generation.Internal;
using Yardarm.Internal;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetRestoreProcessor
    {
        public const string NetStandardFramework = ".NETStandard";
        public static readonly Version NetStandard20 = new(2, 0, 0, 0);

        private readonly PackageSpec _packageSpec;
        private readonly YardarmAssemblyLoadContext _assemblyLoadContext;
        private readonly ILogger<NuGetReferenceGenerator> _logger;

        public NuGetRestoreProcessor(PackageSpec packageSpec, YardarmAssemblyLoadContext assemblyLoadContext,
            ILogger<NuGetReferenceGenerator> logger)
        {
            _packageSpec = packageSpec ?? throw new ArgumentNullException(nameof(packageSpec));
            _assemblyLoadContext = assemblyLoadContext ?? throw new ArgumentNullException(nameof(assemblyLoadContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NuGetRestoreInfo> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                using var cacheContext = new SourceCacheContext();

                _packageSpec.RestoreMetadata = new ProjectRestoreMetadata
                {
                    OutputPath = tempPath,
                    ProjectName = _packageSpec.Name,
                    ProjectStyle = ProjectStyle.PackageReference
                };

                var settings = Settings.LoadDefaultSettings(tempPath);

                var logger = new NuGetLogger(_logger);

                var dependencyProviders = RestoreCommandProviders.Create(
                    SettingsUtility.GetGlobalPackagesFolder(settings),
                    Enumerable.Empty<string>(),
                    SettingsUtility.GetEnabledSources(settings).Select(source =>
                        Repository.Factory.GetCoreV3(source.Source)),
                    cacheContext,
                    new LocalPackageFileCache(),
                    logger);

                var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);
                var packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(settings);

                var restoreRequest = new RestoreRequest(_packageSpec, dependencyProviders, cacheContext,
                    clientPolicyContext, packageSourceMapping, logger, new LockFileBuilderCache())
                {
                    ProjectStyle = ProjectStyle.PackageReference, RestoreOutputPath = tempPath
                };

                var restoreCommand = new RestoreCommand(restoreRequest);

                var result = await restoreCommand.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (!result.Success)
                {
                    throw new NuGetRestoreException(result);
                }

                return new NuGetRestoreInfo
                {
                    Result = result,
                    Providers = dependencyProviders,
                    SourceGenerators = CollectAnalyzers(dependencyProviders, result)
                };
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }

        // Collect C# analyzers from the direct NuGet dependencies (ignores transitive dependencies)
        private List<ISourceGenerator> CollectAnalyzers(RestoreCommandProviders dependencyProviders, RestoreResult result)
        {
            var generators = new List<ISourceGenerator>();

            LockFileTarget netstandardTarget = result.LockFile.Targets
                .First(p => p.TargetFramework.Framework == NetStandardFramework &&
                            p.TargetFramework.Version == NetStandard20);

            foreach (var directDependency in _packageSpec.Dependencies)
            {
                // Get the exact version we restored
                var version = netstandardTarget.Libraries.FirstOrDefault(p => p.Name == directDependency.Name)?.Version;
                if (version is not null)
                {
                    var localPackageInfo =
                        dependencyProviders.GlobalPackages.FindPackage(directDependency.Name, version);

                    // For now, we explicitly only handle Roslyn 4.0 analyzers
                    foreach (var file in localPackageInfo.Files.Where(p => p.StartsWith("analyzers/dotnet/roslyn4.0/cs/")))
                    {
                        // Ignore resource assemblies, just look in the root
                        var suffix = file.Substring("analyzers/dotnet/roslyn4.0/cs/".Length);
                        if (!suffix.Contains('/'))
                        {
                            generators.AddRange(GetGenerators(Path.Join(localPackageInfo.ExpandedPath, file)));
                        }
                    }
                }
            }

            return generators;
        }

        // Instantiate source generators from an analyzer assembly
        private IEnumerable<ISourceGenerator> GetGenerators(string file)
        {
            var assembly = _assemblyLoadContext.LoadFromAssemblyPath(file);

            var generatorTypes = assembly.ExportedTypes.Where(p =>
                p.IsClass && !p.IsGenericTypeDefinition && !p.IsAbstract
                && p.GetCustomAttribute<GeneratorAttribute>() != null);

            foreach (var generatorType in generatorTypes)
            {
                switch (Activator.CreateInstance(generatorType))
                {
                    case ISourceGenerator sourceGenerator:
                        yield return sourceGenerator;
                        break;

                    case IIncrementalGenerator incrementalGenerator:
                        yield return incrementalGenerator.AsSourceGenerator();
                        break;
                }
            }
        }
    }
}
