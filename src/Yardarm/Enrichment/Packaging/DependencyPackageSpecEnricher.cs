using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.ProjectModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.Enrichment.Packaging
{
    internal class DependencyPackageSpecEnricher : IPackageSpecEnricher
    {
        private readonly IDependencyGenerator[] _dependencyGenerators;

        public DependencyPackageSpecEnricher(IEnumerable<IDependencyGenerator> dependencyGenerators)
        {
            _dependencyGenerators = dependencyGenerators.ToArray();
        }

        public PackageSpec Enrich(PackageSpec packageSpec)
        {
            foreach (TargetFrameworkInformation targetFramework in packageSpec.TargetFrameworks)
            {
                targetFramework.Dependencies.AddRange(_dependencyGenerators
                    .SelectMany(p => p.GetDependencies(targetFramework.FrameworkName)));

                if (targetFramework.FrameworkName.Framework == NuGetFrameworkConstants.NetCoreApp)
                {
                    targetFramework.FrameworkReferences.Add(new FrameworkDependency("Microsoft.NETCore.App",
                        FrameworkDependencyFlags.All));

                    var frameworkVersion = targetFramework.FrameworkName.Version;
                    targetFramework.DownloadDependencies.Add(new DownloadDependency("Microsoft.NETCore.App.Ref",
                        new VersionRange(
                            minVersion: new NuGetVersion(frameworkVersion.Major, frameworkVersion.Minor, 0),
                            maxVersion: new NuGetVersion(frameworkVersion.Major, frameworkVersion.Minor + 1, 0),
                            includeMaxVersion: false)));
                }
                else if (targetFramework.FrameworkName.Framework == NuGetFrameworkConstants.NetStandardFramework
                         && targetFramework.FrameworkName.Version == NuGetFrameworkConstants.NetStandard21)
                {
                    // Note that .NET Standard 2.0 is a library reference added by the StandardDependencyGenerator,
                    // whereas .NET Standard 2.1 is a framework reference.

                    targetFramework.FrameworkReferences.Add(new FrameworkDependency("NETStandard.Library",
                        FrameworkDependencyFlags.All));

                    targetFramework.DownloadDependencies.Add(new DownloadDependency("NETStandard.Library.Ref",
                        new VersionRange(
                            minVersion: new NuGetVersion(2, 1, 0),
                            maxVersion: new NuGetVersion(2, 1, 0),
                            includeMaxVersion: true)));
                }
            }

            return packageSpec;
        }
    }
}
