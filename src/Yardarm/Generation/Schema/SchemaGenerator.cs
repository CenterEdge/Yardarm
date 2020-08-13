using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class SchemaGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiSchema> _typeGeneratorRegistry;

        public SchemaGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiSchema> typeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _typeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
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
