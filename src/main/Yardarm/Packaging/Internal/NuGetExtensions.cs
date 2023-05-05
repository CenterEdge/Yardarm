using System.Collections.Generic;
using System.Linq;
using NuGet.Commands;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace Yardarm.Packaging.Internal
{
    internal static class NuGetExtensions
    {
        public static IEnumerable<string> GetFrameworkReferenceDirectories(
            this TargetFrameworkInformation frameworkInformation, NuGetRestoreInfo restoreInfo) =>
            frameworkInformation.GetFrameworkReferenceDirectories(restoreInfo.Providers);

        public static IEnumerable<string> GetFrameworkReferenceDirectories(
            this TargetFrameworkInformation frameworkInformation, RestoreCommandProviders providers) =>
            frameworkInformation.FrameworkReferences
                .Select(frameworkReference =>
                {
                    string refAssemblyName = $"{frameworkReference.Name}.Ref";

                    var version = frameworkInformation.FrameworkName.Version;
                    var versionRange = new VersionRange(
                        new NuGetVersion(version),
                        maxVersion: new NuGetVersion(version.Major, version.Minor + 1, 0),
                        includeMaxVersion: false);

                    return providers.GlobalPackages.FindPackagesById(refAssemblyName)
                        .Where(package => versionRange.Satisfies(package.Version))
                        .MaxBy(package => package.Version)?.ExpandedPath;
                })
                .Where(directory => directory is not null)!;
    }
}
