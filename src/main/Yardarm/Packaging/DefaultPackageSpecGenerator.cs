using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Versioning;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Packaging;

namespace Yardarm.Packaging
{
    public class DefaultPackageSpecGenerator : IPackageSpecGenerator
    {
        private readonly YardarmGenerationSettings _settings;

        protected IList<IPackageSpecEnricher> Enrichers { get; }

        public DefaultPackageSpecGenerator(YardarmGenerationSettings settings, IEnumerable<IPackageSpecEnricher> enrichers)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(enrichers);

            _settings = settings;
            Enrichers = [.. enrichers];
        }

        public virtual PackageSpec Generate() =>
            new PackageSpec(
                [
                    .._settings.TargetFrameworkMonikers
                        .Select(tfm => CreateTargetFrameworkInformation(NuGetFramework.Parse(tfm)))
                ])
            {
                Name = _settings.AssemblyName,
                FilePath = _settings.AssemblyName,
                Dependencies = new List<LibraryDependency>()
            }.Enrich(Enrichers);

        private static TargetFrameworkInformation CreateTargetFrameworkInformation(NuGetFramework frameworkName)
        {
            List<FrameworkDependency> frameworkDependencies = [];
            ImmutableArray<DownloadDependency> downloadDependencies = [];

            if (frameworkName.Framework == NuGetFrameworkConstants.NetCoreApp)
            {
                frameworkDependencies.Add(new FrameworkDependency("Microsoft.NETCore.App",
                    FrameworkDependencyFlags.All));

                var frameworkVersion = frameworkName.Version;
                downloadDependencies = downloadDependencies.Add(new DownloadDependency("Microsoft.NETCore.App.Ref",
                    new VersionRange(
                        minVersion: new NuGetVersion(frameworkVersion.Major, frameworkVersion.Minor, 0),
                        maxVersion: new NuGetVersion(frameworkVersion.Major, frameworkVersion.Minor + 1, 0),
                        includeMaxVersion: false)));
            }
            else if (frameworkName.Framework == NuGetFrameworkConstants.NetStandardFramework
                     && frameworkName.Version == NuGetFrameworkConstants.NetStandard21)
            {
                // Note that .NET Standard 2.0 is a library reference added by the StandardDependencyGenerator,
                // whereas .NET Standard 2.1 is a framework reference.

                frameworkDependencies.Add(new FrameworkDependency("NETStandard.Library",
                    FrameworkDependencyFlags.All));

                downloadDependencies = downloadDependencies.Add(new DownloadDependency("NETStandard.Library.Ref",
                    new VersionRange(
                        minVersion: new NuGetVersion(2, 1, 0),
                        maxVersion: new NuGetVersion(2, 1, 0),
                        includeMaxVersion: true)));
            }

            return new TargetFrameworkInformation
            {
                FrameworkName = frameworkName,
                FrameworkReferences = frameworkDependencies,
                DownloadDependencies = downloadDependencies
            };
        }
    }
}
