using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.Packaging;
using Yardarm.Spec;

namespace Yardarm
{
    public class GenerationContext
    {
        private readonly Lazy<OpenApiDocument> _openApiDocument;
        private readonly Lazy<IOpenApiElementRegistry> _elementRegistry;
        private readonly Lazy<INamespaceProvider> _namespaceProvider;
        private readonly Lazy<INameFormatterSelector> _nameFormatterSelector;
        private readonly Lazy<ITypeGeneratorRegistry> _typeGeneratorRegistry;

        public OpenApiDocument Document => _openApiDocument.Value;
        public IOpenApiElementRegistry ElementRegistry => _elementRegistry.Value;
        public IServiceProvider GenerationServices { get; }
        public INamespaceProvider NamespaceProvider => _namespaceProvider.Value;
        public INameFormatterSelector NameFormatterSelector => _nameFormatterSelector.Value;

        /// <summary>
        /// Details about the NuGet restore operation, once it is completed.
        /// </summary>
        public NuGetRestoreInfo? NuGetRestoreInfo { get; set; }

        public NuGetFramework? CurrentTargetFramework { get; set; }

        public ITypeGeneratorRegistry TypeGeneratorRegistry => _typeGeneratorRegistry.Value;

        public GenerationContext(IServiceProvider serviceProvider)
        {
            GenerationServices = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _openApiDocument = new Lazy<OpenApiDocument>(serviceProvider.GetRequiredService<OpenApiDocument>);
            _elementRegistry = new Lazy<IOpenApiElementRegistry>(serviceProvider.GetRequiredService<IOpenApiElementRegistry>);
            _namespaceProvider = new Lazy<INamespaceProvider>(serviceProvider.GetRequiredService<INamespaceProvider>);
            _nameFormatterSelector =
                new Lazy<INameFormatterSelector>(serviceProvider.GetRequiredService<INameFormatterSelector>);
            _typeGeneratorRegistry =
                new Lazy<ITypeGeneratorRegistry>(serviceProvider.GetRequiredService<ITypeGeneratorRegistry>);
        }
    }
}
