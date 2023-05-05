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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging.Signing;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.Versioning;
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
        public IEnumerable<ISourceGenerator> GetSourceGenerators(RestoreCommandProviders dependencyProviders, LockFile lockFile,
            NuGetFramework targetFramework, AssemblyLoadContext assemblyLoadContext)
        {
            LockFileTarget? lockFileTarget = lockFile.Targets?
                .FirstOrDefault(p => p.TargetFramework == targetFramework);
            if (lockFileTarget is null)
            {
                yield break;
            }

            TargetFrameworkInformation? frameworkInformation = _packageSpec.TargetFrameworks
                .FirstOrDefault(p => p.FrameworkName == targetFramework);

            foreach (LibraryDependency directDependency in _packageSpec.Dependencies
                         .Concat(frameworkInformation?.Dependencies ?? Enumerable.Empty<LibraryDependency>()))
            {
                // Get the exact version we restored
                NuGetVersion? version = lockFileTarget.Libraries
                    .FirstOrDefault(p => string.Equals(p.Name, directDependency.Name, StringComparison.OrdinalIgnoreCase))?
                    .Version;
                if (version is not null)
                {
                    NuGet.Repositories.LocalPackageInfo localPackageInfo =
                        dependencyProviders.GlobalPackages.FindPackage(directDependency.Name, version);

                    foreach (ISourceGenerator generator in GetSourceGenerators(localPackageInfo.ExpandedPath, localPackageInfo.Files, assemblyLoadContext))
                    {
                        yield return generator;
                    }
                }
            }

            if (frameworkInformation is not null)
            {
                foreach (string frameworkDirectory in frameworkInformation.GetFrameworkReferenceDirectories(
                             dependencyProviders))
                {
                    string[] files = Directory.GetFiles(frameworkDirectory, "*.dll",
                        new EnumerationOptions {IgnoreInaccessible = true, RecurseSubdirectories = true});

                    IEnumerable<string> relativeFilePaths = files.Select(
                        p => Path.GetRelativePath(frameworkDirectory, p));

                    foreach (ISourceGenerator generator in GetSourceGenerators(frameworkDirectory, relativeFilePaths, assemblyLoadContext))
                    {
                        yield return generator;
                    }
                }
            }
        }

        private static IEnumerable<ISourceGenerator> GetSourceGenerators(string basePath, IEnumerable<string> files,
            AssemblyLoadContext assemblyLoadContext)
        {
            // Find the highest version of Roslyn listed in the package that is less than equal to
            // the version of Roslyn we are using. Failing that, fallback to include unversioned
            // analyzers.

            // Determine the version of Roslyn we are using
            Version roslynVersion = typeof(CSharpCompilation).Assembly.GetName().Version!;

            IEnumerable<Match> analyzerFiles = files
                .Select(p => Regex.Match(p, @"^(analyzers[/\\]dotnet[/\\](?:roslyn(\d+\.\d+)[/\\])?cs[/\\][^/\\]+\.dll$)"))
                .Where(p => p.Success);

            var filesGroupedByVersion = analyzerFiles
                .Select(p =>
                {
                    if (p.Groups[2].Success)
                    {
                        Version.TryParse(p.Groups[2].Value, out Version? parsedVersion);

                        return (Version: parsedVersion, File: p.Groups[1].Value);
                    }
                    else
                    {
                        return (Version: new Version(0, 0), File: p.Groups[1].Value);
                    }
                })
                .Where(p => p.Version is not null)
                .GroupBy(p => p.Version!)
                .OrderByDescending(p => p.Key);

            var bestMatch = filesGroupedByVersion
                .FirstOrDefault(p => p.Key <= roslynVersion);

            if (bestMatch is not null)
            {
                foreach (ISourceGenerator generator in bestMatch
                             .SelectMany(p => GetSourceGenerators(
                                 Path.Join(basePath, p.File),
                                 assemblyLoadContext)))
                {
                    yield return generator;
                }
            }
        }

        // Instantiate source generators from an analyzer assembly
        private static IEnumerable<ISourceGenerator> GetSourceGenerators(string file, AssemblyLoadContext assemblyLoadContext)
        {
            using var resolver = new PathAssemblyResolver(assemblyLoadContext, Path.GetDirectoryName(file)!);

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

        private class PathAssemblyResolver : IDisposable
        {
            private readonly AssemblyLoadContext _assemblyLoadContext;
            private readonly string _basePath;

            public PathAssemblyResolver(AssemblyLoadContext assemblyLoadContext, string basePath)
            {
                _assemblyLoadContext = assemblyLoadContext;
                _basePath = basePath;

                _assemblyLoadContext.Resolving += Resolving;
            }

            private Assembly? Resolving(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
            {
                string fileName = Path.Combine(_basePath, $"{assemblyName.Name}.dll");
                if (File.Exists(fileName))
                {
                    return assemblyLoadContext.LoadFromAssemblyPath(fileName);
                }

                return null;
            }

            public void Dispose()
            {
                _assemblyLoadContext.Resolving -= Resolving;
            }
        }
    }
}
