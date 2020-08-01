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
        private readonly IResponseSchemaGenerator _responseSchemaGenerator;

        public ResponseGenerator(OpenApiDocument document, IResponseSchemaGenerator responseSchemaGenerator)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _responseSchemaGenerator = responseSchemaGenerator ?? throw new ArgumentNullException(nameof(responseSchemaGenerator));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in _document.Components.Responses
                .Select(p => Generate(p.Value.CreateRoot(p.Key))!)
                .Where(p => p != null))
            {
                yield return syntaxTree;
            }

            foreach (var syntaxTree in _document.Paths
                .Select(p => p.Value.CreateRoot(p.Key))
                .SelectMany(p => p.Element.Operations,
                    (path, operation) =>
                        path.CreateChild(operation.Value, operation.Key.ToString()))
                .SelectMany(p => p.Element.Responses,
                    (operation, response) => operation.CreateChild(response.Value, response.Key))
                .Select(p => Generate(p)!)
                .Where(p => p != null))
            {
                yield return syntaxTree;
            }
        }

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiResponse> requestBody) =>
            _responseSchemaGenerator.GenerateSyntaxTree(requestBody);
    }
}
