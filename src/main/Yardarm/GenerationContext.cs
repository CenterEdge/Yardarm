using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.Packaging;
using Yardarm.Spec;

namespace Yardarm
{
    public class GenerationContext : YardarmContext
    {
        private readonly Lazy<OpenApiDocument> _openApiDocument;
        private readonly Lazy<IOpenApiElementRegistry> _elementRegistry;
        private readonly Lazy<INamespaceProvider> _namespaceProvider;
        private readonly Lazy<INameFormatterSelector> _nameFormatterSelector;
        private readonly Lazy<ITypeGeneratorRegistry> _typeGeneratorRegistry;

        private NuGetFramework _currentTargetFramework = NuGetFramework.UnsupportedFramework;
        private HashSet<string>? _preprocessorSymbols;

        public OpenApiDocument Document => _openApiDocument.Value;
        public IOpenApiElementRegistry ElementRegistry => _elementRegistry.Value;
        public INamespaceProvider NamespaceProvider => _namespaceProvider.Value;
        public INameFormatterSelector NameFormatterSelector => _nameFormatterSelector.Value;

        public ITypeGeneratorRegistry TypeGeneratorRegistry => _typeGeneratorRegistry.Value;

        public NuGetFramework CurrentTargetFramework
        {
            get => _currentTargetFramework;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _currentTargetFramework = value;
                _preprocessorSymbols = null;
            }
        }

        public IReadOnlySet<string> PreprocessorSymbols =>
            _preprocessorSymbols ??= GetPreprocessorSymbols(CurrentTargetFramework);

        public GenerationContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _openApiDocument = new Lazy<OpenApiDocument>(serviceProvider.GetRequiredService<OpenApiDocument>);
            _elementRegistry = new Lazy<IOpenApiElementRegistry>(serviceProvider.GetRequiredService<IOpenApiElementRegistry>);
            _namespaceProvider = new Lazy<INamespaceProvider>(serviceProvider.GetRequiredService<INamespaceProvider>);
            _nameFormatterSelector =
                new Lazy<INameFormatterSelector>(serviceProvider.GetRequiredService<INameFormatterSelector>);
            _typeGeneratorRegistry =
                new Lazy<ITypeGeneratorRegistry>(serviceProvider.GetRequiredService<ITypeGeneratorRegistry>);
        }

        private static HashSet<string> GetPreprocessorSymbols(NuGetFramework targetFramework) =>
            new(targetFramework switch
            {
                { Framework: NuGetFrameworkConstants.NetStandardFramework } =>
                    GetNetStandardPreprocessorSymbols(targetFramework.Version),
                { Framework: NuGetFrameworkConstants.NetCoreApp, Version.Major: < 5 } =>
                    GetNetCoreAppPreprocessorSymbols(targetFramework.Version),
                { Framework: NuGetFrameworkConstants.NetCoreApp, Version.Major: >= 5 } =>
                    GetNetPreprocessorSymbols(targetFramework.Version),
                _ => Enumerable.Empty<string>()
            });

        private static IEnumerable<string> GetNetStandardPreprocessorSymbols(Version frameworkVersion)
        {
            var result = new List<string> { "NETSTANDARD", $"NETSTANDARD{frameworkVersion.Major}_{frameworkVersion.Minor}" };

            foreach (var version in new Version[] { new(2, 0), new(2, 1) }
                         .Where(p => p <= frameworkVersion))
            {
                result.Add($"NETSTANDARD{version.Major}_{version.Minor}_OR_GREATER");
            }

            return result;
        }

        private static IEnumerable<string> GetNetCoreAppPreprocessorSymbols(Version frameworkVersion)
        {
            var result = new List<string>
            {
                "NETCOREAPP", $"NETCOREAPP{frameworkVersion.Major}_{frameworkVersion.Minor}"
            };

            foreach (var version in new Version[] { new(2, 0), new(2, 1), new(3, 0), new(3, 1) }
                         .Where(p => p <= frameworkVersion))
            {
                result.Add($"NETCOREAPP{version.Major}_{version.Minor}_OR_GREATER");
            }

            return result;
        }

        private static IEnumerable<string> GetNetPreprocessorSymbols(Version frameworkVersion)
        {
            var result = new List<string> { $"NET{frameworkVersion.Major}_{frameworkVersion.Minor}" };

            foreach (var version in new Version[] { new(5, 0), new(6, 0), new(7, 0), new(8, 0) }
                         .Where(p => p <= frameworkVersion))
            {
                result.Add($"NET{version.Major}_{version.Minor}_OR_GREATER");
            }

            // Also include all .NET Core 3.1 symbols except NETCOREAPP3_1
            result.AddRange(GetNetCoreAppPreprocessorSymbols(new Version(3, 1)).Except(new[] { "NETCOREAPP3_1" }));

            return result;
        }
    }
}
