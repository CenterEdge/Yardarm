using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestBodyGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiRequestBody> _requestBodyGeneratorRegistry;

        public RequestBodyGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiRequestBody> requestBodyGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _requestBodyGeneratorRegistry = requestBodyGeneratorRegistry ?? throw new ArgumentNullException(nameof(requestBodyGeneratorRegistry));
        }

        public void Preprocess()
        {
            foreach (var request in GetRequestBodies())
            {
                Preprocess(request);
            }
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetRequestBodies()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<LocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies() =>
            _document.Components.RequestBodies
                .Select(p => p.Value.CreateRoot(p.Key))
                .Concat(_document.Paths.ToLocatedElements()
                    .GetOperations()
                    .GetRequestBodies()
                    .Where(p => p.Element.Reference == null));

        protected virtual void Preprocess(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            _requestBodyGeneratorRegistry.Get(requestBody).Preprocess();

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            _requestBodyGeneratorRegistry.Get(requestBody).GenerateSyntaxTree();
    }
}
