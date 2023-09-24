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
        private readonly ITypeGeneratorRegistry<OpenApiUnknownResponse> _unknownResponseGeneratorRegistry;

        public ResponseSetGenerator(OpenApiDocument document,
            ITypeGeneratorRegistry<OpenApiResponses> responsesGeneratorRegistry,
            ITypeGeneratorRegistry<OpenApiUnknownResponse> unknownResponseGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(responsesGeneratorRegistry);
            ArgumentNullException.ThrowIfNull(unknownResponseGeneratorRegistry);

            _document = document;
            _responsesGeneratorRegistry = responsesGeneratorRegistry;
            _unknownResponseGeneratorRegistry = unknownResponseGeneratorRegistry;
        }

        public IEnumerable<SyntaxTree> Generate() =>
            GetResponseSets()
                .Select(Generate)
                .Concat(
                    GetResponseSets()
                        .Select(p => p.GetUnknownResponse())
                        .Select(Generate))
                .Where(p => p != null)!;

        private IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets() =>
            _document.Paths.ToLocatedElements()
                .GetOperations()
                .GetResponseSets();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiResponses> responseSet) =>
            _responsesGeneratorRegistry.Get(responseSet).GenerateSyntaxTree();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiUnknownResponse> unknownResponse) =>
            _unknownResponseGeneratorRegistry.Get(unknownResponse).GenerateSyntaxTree();
    }
}
