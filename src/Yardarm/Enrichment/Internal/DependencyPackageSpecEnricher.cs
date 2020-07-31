using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;
using NuGet.ProjectModel;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Internal
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
            packageSpec.Dependencies.AddRange(_dependencyGenerators
                .SelectMany(p => p.GetDependencies()));

            return packageSpec;
        }
    }
}
