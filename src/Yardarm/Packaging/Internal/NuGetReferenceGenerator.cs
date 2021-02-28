using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Yardarm.Generation;
using Yardarm.Generation.Internal;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetReferenceGenerator : IReferenceGenerator
    {
        private const string NetStandardFramework = ".NETStandard";
        private static readonly Version NetStandard20 = new Version(2, 0, 0, 0);
        private const string NetStandardLibrary = "NETStandard.Library";

        private readonly PackageSpec _packageSpec;
        private readonly ILogger<NuGetReferenceGenerator> _logger;

        public NuGetReferenceGenerator(PackageSpec packageSpec, ILogger<NuGetReferenceGenerator> logger)
        {
            _packageSpec = packageSpec ?? throw new ArgumentNullException(nameof(packageSpec));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async IAsyncEnumerable<MetadataReference> Generate([EnumeratorCancellation] CancellationToken cancellationToken = default)
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

                var restoreRequest = new RestoreRequest(_packageSpec, dependencyProviders, cacheContext, null, logger)
                {
                    ProjectStyle = ProjectStyle.PackageReference,
                    RestoreOutputPath = tempPath
                };

                var restoreCommand = new RestoreCommand(restoreRequest);

                var result = await restoreCommand.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (!result.Success)
                {
                    throw new NuGetRestoreException(result);
                }

                var dependencies = ExtractDependencies(result.LockFile);
                foreach (string dependency in dependencies)
                {
                    yield return MetadataReference.CreateFromFile(dependency);
                }
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }

        private IEnumerable<string> ExtractDependencies(LockFile lockFile)
        {
            // Get the libraries to import for targeting netstandard2.0
            LockFileTarget netstandardTarget = lockFile.Targets
                .First(p => p.TargetFramework.Framework == NetStandardFramework &&
                            p.TargetFramework.Version == NetStandard20);

            // Collect all DLL files from CompileTimeAssemblies from that target
            // Note that we apply File.Exists since there may be muliple paths we're searching for each file listed
            List<string> dependencies = netstandardTarget.Libraries
                .SelectMany(p => p.CompileTimeAssemblies.Select(q => new
                {
                    Library = p,
                    Path = q.Path.Replace('/', Path.DirectorySeparatorChar)
                }))
                .Where(p => Path.GetExtension(p.Path) == ".dll")
                .SelectMany(
                    _ => lockFile.PackageFolders.Select(p => p.Path),
                    (dependency, folder) =>
                        Path.Combine(folder, dependency.Library.Name, dependency.Library.Version.ToString(), dependency.Path)
                )
                .Where(File.Exists)
                .ToList();

            // NETStandard.Library is a bit different, it has reference assemblies in the build/netstandard2.0/ref directory
            // which are imported via a MSBuild target file in the package. So we need to emulate that behavior here.

            LockFileTargetLibrary netstandardLibrary = netstandardTarget.Libraries.First(p => p.Name == NetStandardLibrary);

            string refDirectory = lockFile.PackageFolders.Select(p => p.Path)
                .Select(p => Path.Combine(p, netstandardLibrary.Name, netstandardLibrary.Version.ToString()))
                .First(Directory.Exists);
            refDirectory = Path.Combine(refDirectory, @"build\netstandard2.0\ref");

            dependencies.AddRange(Directory.EnumerateFiles(refDirectory, "*.dll"));

            return dependencies;
        }
    }
}
