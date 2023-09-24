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
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(responseGeneratorRegistry);

            _document = document;
            _responseGeneratorRegistry = responseGeneratorRegistry;
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

        private IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses() =>
            _document.Components.Responses
                .Select(p => p.Value.CreateRoot(p.Key))
                .Concat(_document.Paths.ToLocatedElements()
                    .GetOperations()
                    .GetResponseSets()
                    .GetResponses());

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiResponse> response) =>
            _responseGeneratorRegistry.Get(response).GenerateSyntaxTree();
    }
}
