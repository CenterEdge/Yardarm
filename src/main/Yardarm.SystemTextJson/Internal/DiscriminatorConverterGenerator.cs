using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterGenerator(
        OpenApiDocument document,
        [FromKeyedServices(DiscriminatorConverterTypeGenerator.GeneratorCategory)] ITypeGeneratorRegistry<OpenApiSchema> converterTypeGeneratorRegistry,
        IOperationNameProvider operationNameProvider) : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            var schemas = document
                .GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider)
                .Where(schema => SchemaHelper.IsPolymorphic(schema.Element));

            foreach (var schema in schemas)
            {
                var converterGenerator = converterTypeGeneratorRegistry.Get(schema);

                var syntaxTree = converterGenerator.GenerateSyntaxTree();
                if (syntaxTree != null)
                {
                    yield return syntaxTree;
                }
            }
        }
    }
}
