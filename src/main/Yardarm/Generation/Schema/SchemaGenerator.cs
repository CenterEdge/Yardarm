using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class SchemaGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<IOpenApiSchema> _typeGeneratorRegistry;

        public SchemaGenerator(OpenApiDocument document, ITypeGeneratorRegistry<IOpenApiSchema> typeGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(typeGeneratorRegistry);

            _document = document;
            _typeGeneratorRegistry = typeGeneratorRegistry;
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var schema in _document.Components.Schemas)
            {
                var element = schema.Value.CreateRoot(schema.Key);

                var generator = _typeGeneratorRegistry.Get(element);

                var syntaxTree = generator.GenerateSyntaxTree();
                if (syntaxTree != null)
                {
                    yield return syntaxTree;
                }
            }
        }
    }
}
