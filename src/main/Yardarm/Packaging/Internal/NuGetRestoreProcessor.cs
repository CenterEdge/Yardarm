using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Signing;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Yardarm.Generation.Internal;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetRestoreProcessor
    {
        private readonly PackageSpec _packageSpec;
        private readonly YardarmGenerationSettings _settings;
        private readonly ILogger<NuGetReferenceGenerator> _logger;

        public NuGetRestoreProcessor(PackageSpec packageSpec, YardarmGenerationSettings settings,
            ILogger<NuGetReferenceGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(packageSpec);
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            _packageSpec = packageSpec;
            _settings = settings;
            _logger = logger;
        }

        public async Task<NuGetRestoreInfo> ExecuteAsync(bool readLockFileOnly, CancellationToken cancellationToken = default)
        {
            string? intermediatePath = _settings.IntermediateOutputPath;
            bool isTempPath = false;

            if (string.IsNullOrEmpty(intermediatePath))
            {
                intermediatePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(intermediatePath);
                isTempPath = true;
            }
            else if (!Path.IsPathFullyQualified(intermediatePath))
            {
                intermediatePath = Path.Combine(Directory.GetCurrentDirectory(), intermediatePath);
            }

            try
            {
                using var cacheContext = new SourceCacheContext();

                _packageSpec.RestoreMetadata = new ProjectRestoreMetadata
                {
                    OutputPath = intermediatePath,
                    CacheFilePath = Path.Combine(intermediatePath, "project.nuget.cache"),
                    ProjectName = _packageSpec.Name,
                    ProjectPath = $"{_packageSpec.Name}.csproj", // Ensures consistent naming of .nuget.g.props/targets in intermediate directory
                    ProjectStyle = ProjectStyle.PackageReference,
                    CrossTargeting = true,
                };
                _packageSpec.RestoreMetadata.ProjectUniqueName = _packageSpec.RestoreMetadata.ProjectPath;

                var settings = Settings.LoadDefaultSettings(intermediatePath);

                var logger = new NuGetLogger(_logger);

                var dependencyProviders = RestoreCommandProviders.Create(
                    SettingsUtility.GetGlobalPackagesFolder(settings),
                    Enumerable.Empty<string>(),
                    SettingsUtility.GetEnabledSources(settings).Select(source =>
                        Repository.Factory.GetCoreV3(source.Source)),
                    cacheContext,
                    new LocalPackageFileCache(),
                    logger);

                if (!readLockFileOnly)
                {
                    var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);
                    var packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(settings);

                    var dependencyGraphSpec = new DependencyGraphSpec();
                    dependencyGraphSpec.AddProject(_packageSpec);

                    var restoreRequest = new RestoreRequest(_packageSpec, dependencyProviders, cacheContext,
                        clientPolicyContext, packageSourceMapping, logger, new LockFileBuilderCache())
                    {
                        ProjectStyle = ProjectStyle.PackageReference,
                        RestoreOutputPath = intermediatePath,
                        AllowNoOp = !isTempPath,
                        DependencyGraphSpec = dependencyGraphSpec
                    };

                    var restoreCommand = new RestoreCommand(restoreRequest);

                    var result = await restoreCommand.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    if (!result.Success)
                    {
                        throw new NuGetRestoreException(result);
                    }

                    if (!isTempPath) // No need to commit if we're deleting anyway
                    {
                        await result.CommitAsync(logger, cancellationToken).ConfigureAwait(false);
                    }

                    return new NuGetRestoreInfo(dependencyProviders, result.LockFile);
                }
                else
                {
                    var lockFileName = Path.Combine(intermediatePath, LockFileFormat.AssetsFileName);

                    var lockFile = LockFileUtilities.GetLockFile(lockFileName, logger);

                    return new NuGetRestoreInfo(dependencyProviders, lockFile);
                }
            }
            finally
            {
                if (isTempPath)
                {
                    Directory.Delete(intermediatePath, true);
                }
            }
        }

        // Collect C# source generators from the direct NuGet dependencies (ignores transitive dependencies)
        public List<ISourceGenerator> GetSourceGenerators(RestoreCommandProviders dependencyProviders, LockFile lockFile,
            NuGetFramework targetFramework, AssemblyLoadContext assemblyLoadContext)
        {
            var generators = new List<ISourceGenerator>();

            LockFileTarget lockFileTarget = lockFile.Targets
                .First(p => p.TargetFramework == targetFramework);

            foreach (var directDependency in _packageSpec.Dependencies
                         .Concat(_packageSpec.TargetFrameworks
                             .Where(p => p.FrameworkName == targetFramework)
                             .SelectMany(framework => framework.Dependencies)))
            {
                // Get the exact version we restored
                var version = lockFileTarget.Libraries
                    .FirstOrDefault(p => string.Equals(p.Name, directDependency.Name, StringComparison.OrdinalIgnoreCase))?
                    .Version;
                if (version is not null)
                {
                    NuGet.Repositories.LocalPackageInfo localPackageInfo =
                        dependencyProviders.GlobalPackages.FindPackage(directDependency.Name, version);

                    // For now, we explicitly only handle Roslyn 4.0 analyzers or unversioned analyzers
                    // The regex also excludes resource assemblies in nested directories
                    foreach (Match file in localPackageInfo.Files
                                 .Select(p => Regex.Match(p, @"^(analyzers/dotnet/(?:roslyn4\.0/)?cs/[^/]+\.dll$)"))
                                 .Where(p => p.Success))
                    {
                        generators.AddRange(GetSourceGenerators(
                            Path.Join(localPackageInfo.ExpandedPath, file.Groups[1].Value),
                            assemblyLoadContext));
                    }
                }
            }

            return generators;
        }

        // Instantiate source generators from an analyzer assembly
        private static IEnumerable<ISourceGenerator> GetSourceGenerators(string file, AssemblyLoadContext assemblyLoadContext)
        {
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(file);

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
