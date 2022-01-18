using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;
using NuGet.ProjectModel;
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
            }

            return packageSpec;
        }
    }
}
