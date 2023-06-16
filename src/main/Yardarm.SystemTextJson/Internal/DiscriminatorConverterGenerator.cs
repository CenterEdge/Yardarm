﻿using System;
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
        private readonly ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> _converterTypeGeneratorRegistry;

        public DiscriminatorConverterGenerator(OpenApiDocument document,
            ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> converterTypeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _converterTypeGeneratorRegistry = converterTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(converterTypeGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            var schemas = _document
                .GetAllSchemas()
                .Where(schema => JsonDiscriminatorEnricher.IsPolymorphic(schema.Element));

            foreach (var schema in schemas)
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
