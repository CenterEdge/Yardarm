using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using Yardarm.Enrichment;

namespace Yardarm.Generation
{
    public class DefaultPackageSpecGenerator : IPackageSpecGenerator
    {
        private readonly YardarmGenerationSettings _settings;
        private readonly IList<IPackageSpecEnricher> _enrichers;

        public DefaultPackageSpecGenerator(YardarmGenerationSettings settings, IEnumerable<IPackageSpecEnricher> enrichers)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _enrichers = enrichers.ToArray();
        }

        public virtual PackageSpec Generate() =>
            Enrich(new PackageSpec(new[]
            {
                new TargetFrameworkInformation {FrameworkName = NuGetFramework.Parse("netstandard2.0")}
            })
            {
                Name = _settings.AssemblyName,
                Dependencies = new List<LibraryDependency>()
            });

        protected PackageSpec Enrich(PackageSpec spec)
        {
            return _enrichers.Aggregate(spec, (p, enricher) => enricher.Enrich(p));
        }
    }
}
