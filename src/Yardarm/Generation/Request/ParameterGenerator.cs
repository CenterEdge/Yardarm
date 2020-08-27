using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class ParameterGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiParameter> _parameterGeneratorRegistry;

        public ParameterGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiParameter> parameterGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _parameterGeneratorRegistry = parameterGeneratorRegistry ?? throw new ArgumentNullException(nameof(parameterGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in _document.Components.Parameters
                .Select(p => p.Value.CreateRoot(p.Key))
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            _parameterGeneratorRegistry.Get(parameter).GenerateSyntaxTree();
    }
}
