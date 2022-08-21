using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class HeaderGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiHeader> _headerGeneratorRegistry;

        public HeaderGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiHeader> headerGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _headerGeneratorRegistry = headerGeneratorRegistry ?? throw new ArgumentNullException(nameof(headerGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in _document.Components.Headers
                .Select(p => p.Value.CreateRoot(p.Key))
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiHeader> parameter) =>
            _headerGeneratorRegistry.Get(parameter).GenerateSyntaxTree();
    }
}
