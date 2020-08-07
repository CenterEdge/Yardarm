using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Features;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation
{
    public class GenerationContext
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Lazy<OpenApiDocument> _openApiDocument;
        private readonly Lazy<INamespaceProvider> _namespaceProvider;
        private readonly Lazy<ITypeNameGenerator> _typeNameGenerator;
        private readonly Lazy<INameFormatterSelector> _nameFormatterSelector;
        private readonly Lazy<ITypeGeneratorRegistry> _typeGeneratorRegistry;
        private readonly Lazy<IEnrichers> _enrichers;

        public FeatureCollection Features { get; } = new FeatureCollection();

        public OpenApiDocument Document => _openApiDocument.Value;
        public INamespaceProvider NamespaceProvider => _namespaceProvider.Value;
        public ITypeNameGenerator TypeNameGenerator => _typeNameGenerator.Value;
        public INameFormatterSelector NameFormatterSelector => _nameFormatterSelector.Value;
        public ITypeGeneratorRegistry SchemaGeneratorRegistry => _typeGeneratorRegistry.Value;
        public IEnrichers Enrichers => _enrichers.Value;

        public GenerationContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _openApiDocument = new Lazy<OpenApiDocument>(serviceProvider.GetRequiredService<OpenApiDocument>);
            _namespaceProvider = new Lazy<INamespaceProvider>(serviceProvider.GetRequiredService<INamespaceProvider>);
            _typeNameGenerator = new Lazy<ITypeNameGenerator>(serviceProvider.GetRequiredService<ITypeNameGenerator>);
            _nameFormatterSelector =
                new Lazy<INameFormatterSelector>(serviceProvider.GetRequiredService<INameFormatterSelector>);
            _typeGeneratorRegistry =
                new Lazy<ITypeGeneratorRegistry>(serviceProvider.GetRequiredService<ITypeGeneratorRegistry>);
            _enrichers = new Lazy<IEnrichers>(serviceProvider.GetRequiredService<IEnrichers>);
        }
    }
}
