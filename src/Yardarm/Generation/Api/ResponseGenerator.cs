using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
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

        public void Preprocess()
        {
            foreach (var response in GetResponses())
            {
                Preprocess(response);
            }
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
                    .GetResponses());

        protected virtual void Preprocess(LocatedOpenApiElement<OpenApiResponse> requestBody) =>
            _responseGeneratorRegistry.Get(requestBody).Preprocess();

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiResponse> requestBody) =>
            _responseGeneratorRegistry.Get(requestBody).GenerateSyntaxTree();
    }
}
