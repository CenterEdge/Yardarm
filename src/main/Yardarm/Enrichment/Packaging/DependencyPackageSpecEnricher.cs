using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using Yardarm.Packaging;

namespace Yardarm.Enrichment.Packaging;

internal class DependencyPackageSpecEnricher(
    IEnumerable<IDependencyGenerator> dependencyGenerators)
    : IPackageSpecEnricher
{
    private readonly IDependencyGenerator[] _dependencyGenerators = [.. dependencyGenerators];

    public PackageSpec Enrich(PackageSpec packageSpec)
    {
        for (int i = 0; i < packageSpec.TargetFrameworks.Count; i++)
        {
            TargetFrameworkInformation targetFramework = packageSpec.TargetFrameworks[i];

            ImmutableArray<LibraryDependency> newDependencies = targetFramework.Dependencies
                .AddRange(_dependencyGenerators.SelectMany(p => p.GetDependencies(targetFramework.FrameworkName)));

            if (!newDependencies.Equals(targetFramework.Dependencies))
            {
                packageSpec.TargetFrameworks[i] = new TargetFrameworkInformation(targetFramework)
                {
                    Dependencies = newDependencies
                };
            }
        }

        return packageSpec;
    }
}
