using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseSetGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<OpenApiResponses> responsesGeneratorRegistry,
        ITypeGeneratorRegistry<OpenApiUnknownResponse> unknownResponseGeneratorRegistry,
        IOperationNameProvider operationNameProvider)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate() =>
            GetResponseSets()
                .Select(Generate)
                .Concat(
                    GetResponseSets()
                        .Select(p => p.GetUnknownResponse())
                        .Select(Generate))
                .Where(p => p != null)!;

        private IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets() =>
            document.Paths.ToLocatedElements()
                .GetOperations()
                .WhereOperationHasName(operationNameProvider)
                .GetResponseSets();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiResponses> responseSet) =>
            responsesGeneratorRegistry.Get(responseSet).GenerateSyntaxTree();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiUnknownResponse> unknownResponse) =>
            unknownResponseGeneratorRegistry.Get(unknownResponse).GenerateSyntaxTree();
    }
}
