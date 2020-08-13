using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Features;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm
{
    public class GenerationContext
    {
        private readonly Lazy<OpenApiDocument> _openApiDocument;
        private readonly Lazy<IOpenApiElementRegistry> _elementRegistry;
        private readonly Lazy<INamespaceProvider> _namespaceProvider;
        private readonly Lazy<ITypeNameProvider> _typeNameProvider;
        private readonly Lazy<INameFormatterSelector> _nameFormatterSelector;
        private readonly Lazy<ITypeGeneratorRegistry> _typeGeneratorRegistry;

        private CSharpCompilation _compilation;

        public FeatureCollection Features { get; } = new FeatureCollection();

        public CSharpCompilation Compilation
        {
            get => _compilation;
            set => _compilation = value ?? throw new ArgumentNullException(nameof(value));
        }

        public OpenApiDocument Document => _openApiDocument.Value;
        public IOpenApiElementRegistry ElementRegistry => _elementRegistry.Value;
        public IServiceProvider GenerationServices { get; }
        public INamespaceProvider NamespaceProvider => _namespaceProvider.Value;
        public ITypeNameProvider TypeNameProvider => _typeNameProvider.Value;
        public INameFormatterSelector NameFormatterSelector => _nameFormatterSelector.Value;
        public ITypeGeneratorRegistry SchemaGeneratorRegistry => _typeGeneratorRegistry.Value;

        public GenerationContext(IServiceProvider serviceProvider)
        {
            GenerationServices = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _openApiDocument = new Lazy<OpenApiDocument>(serviceProvider.GetRequiredService<OpenApiDocument>);
            _elementRegistry = new Lazy<IOpenApiElementRegistry>(serviceProvider.GetRequiredService<IOpenApiElementRegistry>);
            _namespaceProvider = new Lazy<INamespaceProvider>(serviceProvider.GetRequiredService<INamespaceProvider>);
            _typeNameProvider = new Lazy<ITypeNameProvider>(serviceProvider.GetRequiredService<ITypeNameProvider>);
            _nameFormatterSelector =
                new Lazy<INameFormatterSelector>(serviceProvider.GetRequiredService<INameFormatterSelector>);
            _typeGeneratorRegistry =
                new Lazy<ITypeGeneratorRegistry>(serviceProvider.GetRequiredService<ITypeGeneratorRegistry>);

            var settings = serviceProvider.GetRequiredService<YardarmGenerationSettings>();
            _compilation = CSharpCompilation.Create(settings.AssemblyName)
                .WithOptions(settings.CompilationOptions);
        }
    }
}
