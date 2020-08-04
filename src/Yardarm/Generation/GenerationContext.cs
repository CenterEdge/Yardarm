using System;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation
{
    public class GenerationContext
    {
        public OpenApiDocument Document { get; set; }
        public INamespaceProvider NamespaceProvider { get; }
        public ITypeNameGenerator TypeNameGenerator { get; }
        public INameFormatterSelector NameFormatterSelector { get; }
        public ISchemaGeneratorFactory SchemaGeneratorFactory { get; }
        public IEnrichers Enrichers { get; }

        public GenerationContext(OpenApiDocument document, INamespaceProvider namespaceProvider, ITypeNameGenerator typeNameGenerator,
            INameFormatterSelector nameFormatterSelector, ISchemaGeneratorFactory schemaGeneratorFactory, IEnrichers enrichers)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            NamespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
            TypeNameGenerator = typeNameGenerator ?? throw new ArgumentNullException(nameof(typeNameGenerator));
            NameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            SchemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
            Enrichers = enrichers ?? throw new ArgumentNullException(nameof(enrichers));
        }
    }
}
