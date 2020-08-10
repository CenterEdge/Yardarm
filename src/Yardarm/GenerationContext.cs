using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Features;
using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm
{
    public class GenerationContext
    {
        private readonly Lazy<OpenApiDocument> _openApiDocument;
        private readonly Lazy<INamespaceProvider> _namespaceProvider;
        private readonly Lazy<ITypeNameProvider> _typeNameProvider;
        private readonly Lazy<INameFormatterSelector> _nameFormatterSelector;
        private readonly Lazy<ITypeGeneratorRegistry> _typeGeneratorRegistry;
        private readonly Lazy<IEnrichers> _enrichers;

        public FeatureCollection Features { get; } = new FeatureCollection();

        public OpenApiDocument Document => _openApiDocument.Value;
        public INamespaceProvider NamespaceProvider => _namespaceProvider.Value;
        public ITypeNameProvider TypeNameProvider => _typeNameProvider.Value;
        public INameFormatterSelector NameFormatterSelector => _nameFormatterSelector.Value;
        public ITypeGeneratorRegistry SchemaGeneratorRegistry => _typeGeneratorRegistry.Value;
        public IEnrichers Enrichers => _enrichers.Value;

        public GenerationContext(IServiceProvider serviceProvider)
        {
            _openApiDocument = new Lazy<OpenApiDocument>(serviceProvider.GetRequiredService<OpenApiDocument>);
            _namespaceProvider = new Lazy<INamespaceProvider>(serviceProvider.GetRequiredService<INamespaceProvider>);
            _typeNameProvider = new Lazy<ITypeNameProvider>(serviceProvider.GetRequiredService<ITypeNameProvider>);
            _nameFormatterSelector =
                new Lazy<INameFormatterSelector>(serviceProvider.GetRequiredService<INameFormatterSelector>);
            _typeGeneratorRegistry =
                new Lazy<ITypeGeneratorRegistry>(serviceProvider.GetRequiredService<ITypeGeneratorRegistry>);
            _enrichers = new Lazy<IEnrichers>(serviceProvider.GetRequiredService<IEnrichers>);
        }
    }
}
