using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> _typeGeneratorRegistry;

        public DiscriminatorConverterGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> typeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _typeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var schema in _document.Components.Schemas
                         .Where(schema => schema.Value.Discriminator?.PropertyName != null))
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
