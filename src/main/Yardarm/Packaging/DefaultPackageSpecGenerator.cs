using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
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
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Enrichers = enrichers.ToArray();
        }

        public virtual PackageSpec Generate() =>
            new PackageSpec(_settings.TargetFrameworkMonikers
                .Select(tfm => new TargetFrameworkInformation {FrameworkName = NuGetFramework.Parse(tfm)})
                .ToList())
            {
                Name = _settings.AssemblyName,
                FilePath = _settings.AssemblyName,
                Dependencies = new List<LibraryDependency>()
            }.Enrich(Enrichers);
    }
}
