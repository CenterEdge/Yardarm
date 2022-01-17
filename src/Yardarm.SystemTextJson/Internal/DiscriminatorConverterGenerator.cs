using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Schema;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiSchema> _schemaTypeGeneratorRegistry;
        private readonly ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> _converterTypeGeneratorRegistry;

        public DiscriminatorConverterGenerator(OpenApiDocument document,
            ITypeGeneratorRegistry<OpenApiSchema> schemaTypeGeneratorRegistry,
            ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> converterTypeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _schemaTypeGeneratorRegistry = schemaTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(schemaTypeGeneratorRegistry));
            _converterTypeGeneratorRegistry = converterTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(converterTypeGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            var schemas = _document
                .GetAllSchemas()
                .Where(schema => schema.Element.OneOf.Count > 0);

            foreach (var schema in schemas)
            {
                var schemaGenerator = _schemaTypeGeneratorRegistry.Get(schema);
                if (schemaGenerator is OneOfSchemaGenerator)
                {
                    var converterGenerator = _converterTypeGeneratorRegistry.Get(schema);

                    var syntaxTree = converterGenerator.GenerateSyntaxTree();
                    if (syntaxTree != null)
                    {
                        yield return syntaxTree;
                    }
                }
            }
        }
    }
}
