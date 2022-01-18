using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            // Get the libraries to import for our current target
            LockFileTarget lockFileTarget = lockFile.Targets
                .First(p => p.TargetFramework == _context.CurrentTargetFramework);

            // Collect all DLL files from CompileTimeAssemblies from that target
            // Note that we apply File.Exists since there may be multiple paths we're searching for each file listed
            List<string> dependencies = lockFileTarget.Libraries
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

            // Collect platform reference assemblies, i.e. .NET 6 assemblies
            dependencies.AddRange(lockFileTarget.Libraries
                .Where(package => package.PackageType.Any(type => type.Name == "DotnetPlatform"))
                .Select(package =>
                    _context.NuGetRestoreInfo!.Providers.GlobalPackages.FindPackage(package.Name, package.Version))
                .SelectMany(localPackageInfo =>
                    localPackageInfo.Files
                        .Where(p => p.StartsWith($"ref/{lockFileTarget.TargetFramework.GetShortFolderName()}") &&
                                    p.EndsWith(".dll"))
                        .Select(p => Path.Combine(localPackageInfo.ExpandedPath, p.Replace('/', Path.DirectorySeparatorChar)))));

            LockFileTargetLibrary? netstandardLibrary = lockFileTarget.Libraries.FirstOrDefault(p => p.Name == NetStandardLibrary);
            if (netstandardLibrary is not null)
            {
                // NETStandard.Library is a bit different, it has reference assemblies in the build/netstandard2.0/ref directory
                // which are imported via a MSBuild target file in the package. So we need to emulate that behavior here.

                string refDirectory = lockFile.PackageFolders.Select(p => p.Path)
                    .Select(p => Path.Combine(p, netstandardLibrary.Name.ToLowerInvariant(),
                        netstandardLibrary.Version.ToString()))
                    .First(Directory.Exists);
                refDirectory = Path.Combine(refDirectory, "build", "netstandard2.0", "ref");

                dependencies.AddRange(Directory.EnumerateFiles(refDirectory, "*.dll"));
            }

            return Deduplicate(dependencies);
        }

        // If more than one version of the same assembly is referenced, take the highest version number.
        // This can happen in cases like .NET Standard 2.1 including System.Threading.Tasks.Extensions
        // but some other transitively depending on the System.Threading.Tasks.Extensions NuGet package.
        private IEnumerable<string> Deduplicate(IEnumerable<string> dependencyPaths) =>
            dependencyPaths
                .Select(path => (path, name: AssemblyName.GetAssemblyName(path)))
                .GroupBy(p => (p.name.Name, p.name.CultureName, GetPublicKeyTokenString(p.name)))
                .Select(p => p
                    .OrderByDescending(q => q.name.Version)
                    .Select(q => q.path)
                    .First());
        private static string? GetPublicKeyTokenString(AssemblyName name)
        {
            byte[]? token = name.GetPublicKeyToken();
            if (token == null)
            {
                return null;
            }

            var sb = new StringBuilder(16);
            for (int i = 0; i < token.Length; i++)
            {
                sb.AppendFormat("{0:x2}", token[i]);
            }

            return sb.ToString();
        }
    }
}
