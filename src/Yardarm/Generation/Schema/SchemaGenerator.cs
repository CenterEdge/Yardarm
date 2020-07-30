using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class SchemaGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;

        public SchemaGenerator(OpenApiDocument document, ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _schemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var schema in _document.Components.Schemas)
            {
                var generator = _schemaGeneratorFactory.Create(schema.Key, schema.Value);

                var syntaxTree = generator.Generate(schema.Key, schema.Value);
                if (syntaxTree != null)
                {
                    yield return syntaxTree;
                }
            }
        }
    }
}
