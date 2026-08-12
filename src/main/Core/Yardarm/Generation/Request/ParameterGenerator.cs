using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class ParameterGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<IOpenApiParameter> parameterGeneratorRegistry)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in (document.Components?.Parameters ?? (IEnumerable<KeyValuePair<string, IOpenApiParameter>>)[])
                .Select(p => p.Value.CreateRoot(p.Key))
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<IOpenApiParameter> parameter) =>
            parameterGeneratorRegistry.Get(parameter).GenerateSyntaxTree();
    }
}
