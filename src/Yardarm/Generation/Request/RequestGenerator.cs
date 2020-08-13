using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiOperation> _operationTypeGeneratorRegistry;

        public RequestGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiOperation> operationTypeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _operationTypeGeneratorRegistry = operationTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(operationTypeGeneratorRegistry));
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
            _document.Paths.ToLocatedElements()
                .GetOperations();

        protected virtual SyntaxTree? Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            _operationTypeGeneratorRegistry.Get(operation).GenerateSyntaxTree();
    }
}
