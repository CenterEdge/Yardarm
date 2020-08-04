using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class SchemaGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ISchemaGeneratorRegistry _schemaGeneratorRegistry;

        public SchemaGenerator(OpenApiDocument document, ISchemaGeneratorRegistry schemaGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _schemaGeneratorRegistry = schemaGeneratorRegistry ?? throw new ArgumentNullException(nameof(schemaGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var schema in _document.Components.Schemas)
            {
                var element = schema.Value.CreateRoot(schema.Key);

                var generator = _schemaGeneratorRegistry.Get(element);

                var syntaxTree = generator.GenerateSyntaxTree();
                if (syntaxTree != null)
                {
                    yield return syntaxTree;
                }
            }
        }
    }
}
