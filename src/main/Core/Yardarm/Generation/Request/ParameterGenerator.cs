using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class ParameterGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<OpenApiParameter> parameterGeneratorRegistry)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in document.Components.Parameters
                .Select(p => p.Value.CreateRoot(p.Key))
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            parameterGeneratorRegistry.Get(parameter).GenerateSyntaxTree();
    }
}
