using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class HeaderGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<IOpenApiHeader> _headerGeneratorRegistry;

        public HeaderGenerator(OpenApiDocument document, ITypeGeneratorRegistry<IOpenApiHeader> headerGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(headerGeneratorRegistry);

            _document = document;
            _headerGeneratorRegistry = headerGeneratorRegistry;
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

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<IOpenApiHeader> parameter) =>
            _headerGeneratorRegistry.Get(parameter).GenerateSyntaxTree();
    }
}
