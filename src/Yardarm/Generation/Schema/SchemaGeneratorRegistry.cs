using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class SchemaGeneratorRegistry : ISchemaGeneratorRegistry
    {
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;

        private readonly Dictionary<LocatedOpenApiElement<OpenApiSchema>, ISchemaGenerator> _registry =
            new Dictionary<LocatedOpenApiElement<OpenApiSchema>, ISchemaGenerator>(new SchemaEqualityComparer());

        public SchemaGeneratorRegistry(ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            _schemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
        }

        public ISchemaGenerator Get(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            if (!_registry.TryGetValue(schemaElement, out var generator))
            {
                generator = _schemaGeneratorFactory.Create(schemaElement);

                _registry[schemaElement] = generator;
            }

            return generator;
        }
    }
}
