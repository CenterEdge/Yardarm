using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public class OperationGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiOperation> _operationTypeGeneratorRegistry;

        public OperationGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiOperation> operationTypeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _operationTypeGeneratorRegistry = operationTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(operationTypeGeneratorRegistry));
        }

        public void Preprocess()
        {
            foreach (var operation in GetOperations())
            {
                Preprocess(operation);
            }
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetOperations()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<LocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            _document.Paths
                .Select(p => p.Value.CreateRoot(p.Key))
                .SelectMany(p => p.Element.Operations,
                    (path, operation) =>
                        path.CreateChild(operation.Value, operation.Key.ToString()));

        protected virtual void Preprocess(LocatedOpenApiElement<OpenApiOperation> operation) =>
            _operationTypeGeneratorRegistry.Get(operation).Preprocess();

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            _operationTypeGeneratorRegistry.Get(operation).GenerateSyntaxTree();
    }
}
