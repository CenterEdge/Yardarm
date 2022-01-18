using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectModel;
using NuGet.Versioning;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Packaging;

namespace Yardarm.Packaging
{
    public class NuGetPacker
    {
        private readonly OpenApiDocument _document;
        private readonly YardarmGenerationSettings _settings;
        private readonly PackageSpec _packageSpec;
        private readonly IList<INuGetPackageEnricher> _packageEnrichers;

        public NuGetPacker(OpenApiDocument document, YardarmGenerationSettings settings,
            PackageSpec packageSpec,
            IEnumerable<INuGetPackageEnricher> packageEnrichers)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _packageSpec = packageSpec ?? throw new ArgumentNullException(nameof(packageSpec));
            _packageEnrichers = packageEnrichers.ToArray();
        }

        public void Pack(IList<YardarmCompilationResult> compilationResults, Stream nugetStream)
        {
            var builder = new PackageBuilder
            {
                Id = _settings.AssemblyName,
                Version = new NuGetVersion(_settings.Version,
                    _settings.VersionSuffix?.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries),
                    null, null),
                Description = _settings.AssemblyName,
                Summary = _document.Info.Description,
                Authors = {_settings.Author},
                Repository = _settings.Repository
            };

            builder.Files.AddRange(compilationResults
                .SelectMany(result => new[]
                {
                    new StreamPackageFile(result.DllOutput, $"lib/{result.TargetFramework.GetShortFolderName()}/{_settings.AssemblyName}.dll",
                        result.TargetFramework),
                    new StreamPackageFile(result.XmlDocumentationOutput,
                        $"lib/{result.TargetFramework.GetShortFolderName()}/{_settings.AssemblyName}.xml", result.TargetFramework)
                }));

            builder.DependencyGroups.AddRange(GetDependencyGroups());

            builder = builder.Enrich(_packageEnrichers);

            builder.Save(nugetStream);
        }

        public void PackSymbols(IList<YardarmCompilationResult> compilationResults, Stream nugetSymbolsStream)
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
                Authors = { _settings.Author }
            };

            builder.Files.AddRange(compilationResults
                .SelectMany(result => new[]
                {
                    new StreamPackageFile(result.PdbOutput, $"lib/{result.TargetFramework.GetShortFolderName()}/{_settings.AssemblyName}.pdb",
                        result.TargetFramework),
                }));

            builder.DependencyGroups.AddRange(GetDependencyGroups());

            builder = builder.Enrich(_packageEnrichers);

            builder.Save(nugetSymbolsStream);
        }

        private IEnumerable<PackageDependencyGroup> GetDependencyGroups() =>
            _packageSpec.TargetFrameworks.Select(
                targetFramework => new PackageDependencyGroup(
                    targetFramework.FrameworkName,
                    targetFramework.Dependencies
                        .Where(dependency => dependency.SuppressParent != LibraryIncludeFlags.All)
                        .Select(dependency =>
                            new PackageDependency(dependency.LibraryRange.Name, dependency.LibraryRange.VersionRange))));
    }
}
