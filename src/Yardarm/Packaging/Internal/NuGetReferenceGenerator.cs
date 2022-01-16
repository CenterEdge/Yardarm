using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using NuGet.ProjectModel;
using Yardarm.Generation;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetReferenceGenerator : IReferenceGenerator
    {
        private const string NetStandardLibrary = "NETStandard.Library";

        private readonly GenerationContext _context;

        public NuGetReferenceGenerator(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IAsyncEnumerable<MetadataReference> Generate(CancellationToken cancellationToken = default)
        {
            var result = _context.NuGetRestoreInfo?.Result;
            Debug.Assert(result is not null);

            var dependencies = ExtractDependencies(result.LockFile);

            return dependencies.Select(dependency => MetadataReference.CreateFromFile(dependency)).ToAsyncEnumerable();
        }

        private IEnumerable<string> ExtractDependencies(LockFile lockFile)
        {
            // Get the libraries to import for targeting netstandard2.0
            LockFileTarget netstandardTarget = lockFile.Targets
                .First(p => p.TargetFramework.Framework == NuGetRestoreProcessor.NetStandardFramework &&
                            p.TargetFramework.Version == NuGetRestoreProcessor.NetStandard20);

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
                        Path.Combine(folder, dependency.Library.Name.ToLowerInvariant(), dependency.Library.Version.ToString(), dependency.Path)
                )
                .Where(File.Exists)
                .ToList();

            // NETStandard.Library is a bit different, it has reference assemblies in the build/netstandard2.0/ref directory
            // which are imported via a MSBuild target file in the package. So we need to emulate that behavior here.

            LockFileTargetLibrary netstandardLibrary = netstandardTarget.Libraries.First(p => p.Name == NetStandardLibrary);

            string refDirectory = lockFile.PackageFolders.Select(p => p.Path)
                .Select(p => Path.Combine(p, netstandardLibrary.Name.ToLowerInvariant(), netstandardLibrary.Version.ToString()))
                .First(Directory.Exists);
            refDirectory = Path.Combine(refDirectory, "build", "netstandard2.0", "ref");

            dependencies.AddRange(Directory.EnumerateFiles(refDirectory, "*.dll"));

            return dependencies;
        }
    }
}
