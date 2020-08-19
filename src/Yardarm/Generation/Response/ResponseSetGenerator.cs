using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseSetGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiResponses> _responsesGeneratorRegistry;

        public ResponseSetGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiResponses> responsesGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _responsesGeneratorRegistry = responsesGeneratorRegistry ?? throw new ArgumentNullException(nameof(responsesGeneratorRegistry));
        }

        public IEnumerable<SyntaxTree> Generate() =>
            GetResponses()
                .Select(Generate)
                .Where(p => p != null)!;

        private IEnumerable<LocatedOpenApiElement<OpenApiResponses>> GetResponses() =>
            _document.Paths.ToLocatedElements()
                .GetOperations()
                .Select(p => p.CreateChild(p.Element.Responses, ""));

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiResponses> requestBody) =>
            _responsesGeneratorRegistry.Get(requestBody).GenerateSyntaxTree();
    }
}
