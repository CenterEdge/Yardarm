using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Packaging;

namespace Yardarm.Packaging
{
    public class NuGetPacker
    {
        private readonly OpenApiDocument _document;
        private readonly YardarmGenerationSettings _settings;
        private readonly IList<IDependencyGenerator> _dependencyGenerators;
        private readonly IList<INuGetPackageEnricher> _packageEnrichers;

        public NuGetPacker(OpenApiDocument document, YardarmGenerationSettings settings,
            IEnumerable<IDependencyGenerator> dependencyGenerators,
            IEnumerable<INuGetPackageEnricher> packageEnrichers)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _dependencyGenerators = dependencyGenerators.ToArray();
            _packageEnrichers = packageEnrichers.ToArray();
        }

        public void Pack(Stream dllStream, Stream xmlDocumentationStream, Stream nugetStream)
        {
            var builder = new PackageBuilder
            {
                Id = _settings.AssemblyName,
                Version = new NuGetVersion(_settings.Version,
                    _settings.VersionSuffix?.Split(new [] {'-'}, StringSplitOptions.RemoveEmptyEntries),
                    null, null),
                Description = _settings.AssemblyName,
                Summary = _document.Info.Description,
                Authors = { _settings.Author },
                Repository = _settings.Repository,
                Files =
                {
                    new StreamPackageFile(dllStream, $"lib/netstandard2.0/{_settings.AssemblyName}.dll", "netstandard2.0"),
                    new StreamPackageFile(xmlDocumentationStream, $"lib/netstandard2.0/{_settings.AssemblyName}.xml", "netstandard2.0")
                },
                DependencyGroups =
                {
                    new PackageDependencyGroup(
                        NuGetFramework.Parse("netstandard2.0"),
                        _dependencyGenerators
                            .SelectMany(p => p.GetDependencies())
                            .Select(p => new PackageDependency(p.LibraryRange.Name, p.LibraryRange.VersionRange)))
                }
            };

            builder = builder.Enrich(_packageEnrichers);

            builder.Save(nugetStream);
        }

        public void PackSymbols(Stream pdbStream, Stream nugetSymbolsStream)
        {
            var builder = new PackageBuilder
            {
                Id = _settings.AssemblyName,
                Version = new NuGetVersion(_settings.Version,
                    _settings.VersionSuffix?.Split(new [] {'-'}, StringSplitOptions.RemoveEmptyEntries),
                    null, null),
                PackageTypes =
                {
                    PackageType.SymbolsPackage
                },
                Description = _settings.AssemblyName,
                Summary = _document.Info.Description,
                Authors = { _settings.Author },
                Files =
                {
                    new StreamPackageFile(pdbStream, $"lib/netstandard2.0/{_settings.AssemblyName}.pdb", "netstandard2.0"),
                },
                DependencyGroups =
                {
                    new PackageDependencyGroup(
                        NuGetFramework.Parse("netstandard2.0"),
                        _dependencyGenerators
                            .SelectMany(p => p.GetDependencies())
                            .Select(p => new PackageDependency(p.LibraryRange.Name, p.LibraryRange.VersionRange)))
                }
            };

            builder = builder.Enrich(_packageEnrichers);

            builder.Save(nugetSymbolsStream);
        }
    }
}
