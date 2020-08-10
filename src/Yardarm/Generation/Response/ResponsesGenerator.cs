using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Response
{
    public class ResponsesGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiResponses> _responsesGeneratorRegistry;
        private readonly ResponsesBaseTypeGenerator _responsesBaseTypeGenerator;

        public ResponsesGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiResponses> responsesGeneratorRegistry,
            ResponsesBaseTypeGenerator responsesBaseTypeGenerator)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _responsesGeneratorRegistry = responsesGeneratorRegistry ?? throw new ArgumentNullException(nameof(responsesGeneratorRegistry));
            _responsesBaseTypeGenerator = responsesBaseTypeGenerator ?? throw new ArgumentNullException(nameof(responsesBaseTypeGenerator));
        }

        public void Preprocess()
        {
            foreach (var responses in GetResponses())
            {
                Preprocess(responses);
            }

            _responsesBaseTypeGenerator.Preprocess();
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetResponses()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }

            var baseType = _responsesBaseTypeGenerator.GenerateSyntaxTree();
            if (baseType != null)
            {
                yield return baseType;
            }
        }

        private IEnumerable<LocatedOpenApiElement<OpenApiResponses>> GetResponses() =>
            _document.Paths.ToLocatedElements()
                .GetOperations()
                .Select(p => p.CreateChild(p.Element.Responses, ""));

        protected virtual void Preprocess(LocatedOpenApiElement<OpenApiResponses> requestBody) =>
            _responsesGeneratorRegistry.Get(requestBody).Preprocess();

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiResponses> requestBody) =>
            _responsesGeneratorRegistry.Get(requestBody).GenerateSyntaxTree();
    }
}
