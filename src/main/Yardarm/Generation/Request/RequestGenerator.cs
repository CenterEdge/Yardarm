using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<OpenApiOperation> operationTypeGeneratorRegistry,
        IOperationNameProvider operationNameProvider)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetOperations()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            document.Paths.ToLocatedElements()
                .GetOperations()
                .WhereOperationHasName(operationNameProvider);

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operationTypeGeneratorRegistry.Get(operation).GenerateSyntaxTree();
    }
}
