using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<IOpenApiResponse> responseGeneratorRegistry,
        IOperationNameProvider operationNameProvider)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetResponses()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<IOpenApiResponse>> GetResponses() =>
            (document.Components.Responses ?? [])
                .Select(p => p.Value.CreateRoot(p.Key))
                .Concat(document.Paths.ToLocatedElements()
                    .GetOperations()
                    .WhereOperationHasName(operationNameProvider)
                    .GetResponseSets()
                    .GetResponses());

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<IOpenApiResponse> response) =>
            responseGeneratorRegistry.Get(response).GenerateSyntaxTree();
    }
}
