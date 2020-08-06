using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public class RequestBodyGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly IRequestBodySchemaGenerator _requestBodySchemaGenerator;

        public RequestBodyGenerator(OpenApiDocument document, IRequestBodySchemaGenerator requestBodySchemaGenerator)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _requestBodySchemaGenerator = requestBodySchemaGenerator ?? throw new ArgumentNullException(nameof(requestBodySchemaGenerator));
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
                .Concat(_document.Paths
                    .Select(p => p.Value.CreateRoot(p.Key))
                    .SelectMany(p => p.Element.Operations,
                        (path, operation) =>
                            path.CreateChild(operation.Value, operation.Key.ToString()))
                    .Where(p => p.Element.RequestBody != null)
                    .Select(p => p.CreateChild(p.Element.RequestBody, p.Key)));

        protected virtual void Preprocess(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            _requestBodySchemaGenerator.Preprocess(requestBody);

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            _requestBodySchemaGenerator.GenerateSyntaxTree(requestBody);
    }
}
