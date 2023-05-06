using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NuGet.Commands;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace Yardarm.Packaging.Internal
{
    internal class SourceGeneratorLoadContext : IDisposable
    {
        private readonly RestoreCommandProviders _dependencyProviders;
        private readonly AssemblyLoadContext _assemblyLoadContext = new(null, true);
        private string? _loadBasePath;
        private bool _disposed;

        public SourceGeneratorLoadContext(RestoreCommandProviders dependencyProviders)
        {
            ArgumentNullException.ThrowIfNull(dependencyProviders);
            _dependencyProviders = dependencyProviders;

            _assemblyLoadContext.Resolving += Resolving;
        }

        // Collect C# source generators from the direct NuGet dependencies (ignores transitive dependencies)
        public IEnumerable<ISourceGenerator> GetSourceGenerators(PackageSpec packageSpec, LockFile lockFile, NuGetFramework targetFramework)
        {
            LockFileTarget? lockFileTarget = lockFile.Targets?
                .FirstOrDefault(p => p.TargetFramework == targetFramework);
            if (lockFileTarget is null)
            {
                yield break;
            }

            TargetFrameworkInformation? frameworkInformation = packageSpec.TargetFrameworks
                .FirstOrDefault(p => p.FrameworkName == targetFramework);

            foreach (LibraryDependency directDependency in packageSpec.Dependencies
                         .Concat(frameworkInformation?.Dependencies ?? Enumerable.Empty<LibraryDependency>()))
            {
                // Get the exact version we restored
                NuGetVersion? version = lockFileTarget.Libraries
                    .FirstOrDefault(p => string.Equals(p.Name, directDependency.Name, StringComparison.OrdinalIgnoreCase))?
                    .Version;
                if (version is not null)
                {
                    NuGet.Repositories.LocalPackageInfo localPackageInfo =
                        _dependencyProviders.GlobalPackages.FindPackage(directDependency.Name, version);

                    foreach (ISourceGenerator generator in GetSourceGenerators(localPackageInfo.ExpandedPath, localPackageInfo.Files))
                    {
                        yield return generator;
                    }
                }
            }

            // Load analyzers built into the target framework. Note that we load these second in case a NuGet package
            // is overriding one with a newer version.
            if (frameworkInformation is not null)
            {
                foreach (string frameworkDirectory in frameworkInformation.GetFrameworkReferenceDirectories(
                             _dependencyProviders))
                {
                    string[] files = Directory.GetFiles(frameworkDirectory, "*.dll",
                        new EnumerationOptions {IgnoreInaccessible = true, RecurseSubdirectories = true});

                    IEnumerable<string> relativeFilePaths = files.Select(
                        p => Path.GetRelativePath(frameworkDirectory, p));

                    foreach (ISourceGenerator generator in GetSourceGenerators(frameworkDirectory, relativeFilePaths))
                    {
                        yield return generator;
                    }
                }
            }
        }

        private IEnumerable<ISourceGenerator> GetSourceGenerators(string basePath, IEnumerable<string> files)
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
                                 Path.Join(basePath, p.File))))
                {
                    yield return generator;
                }
            }
        }

        // Instantiate source generators from an analyzer assembly
        private IEnumerable<ISourceGenerator> GetSourceGenerators(string file)
        {
            if (_assemblyLoadContext.Assemblies.Any(p => p.GetName().Name == Path.GetFileNameWithoutExtension(file)))
            {
                // Don't reload the same assembly, just keep the first version we load
                yield break;
            }

            _loadBasePath = Path.GetDirectoryName(file)!;
            try
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
            finally
            {
                _loadBasePath = null;
            }
        }

        private Assembly? Resolving(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            // Try to load missing dependencies from the same directory as the file we're currently loading.
            if (_loadBasePath is not null)
            {
                string fileName = Path.Combine(_loadBasePath, $"{assemblyName.Name}.dll");
                if (File.Exists(fileName))
                {
                    return assemblyLoadContext.LoadFromAssemblyPath(fileName);
                }
            }

            return null;
        }

        ~SourceGeneratorLoadContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _assemblyLoadContext.Resolving -= Resolving;
                _assemblyLoadContext.Unload();
                _disposed = true;
            }
        }
    }
}
