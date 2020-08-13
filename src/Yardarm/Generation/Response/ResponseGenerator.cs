using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiResponse> _responseGeneratorRegistry;

        public ResponseGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiResponse> responseGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _responseGeneratorRegistry = responseGeneratorRegistry ?? throw new ArgumentNullException(nameof(responseGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetResponses()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<LocatedOpenApiElement<OpenApiResponse>> GetResponses() =>
            _document.Components.Responses
                .Select(p => p.Value.CreateRoot(p.Key))
                .Concat(_document.Paths.ToLocatedElements()
                    .GetOperations()
                    .GetResponseSets()
                    .GetResponses()
                    .Where(p => p.Element.Reference == null));

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiResponse> response) =>
            _responseGeneratorRegistry.Get(response).GenerateSyntaxTree();
    }
}
