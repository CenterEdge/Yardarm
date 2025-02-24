using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<OpenApiTag> tagGeneratorRegistry,
        [FromKeyedServices(TagImplementationTypeGenerator.GeneratorCategory)] ITypeGeneratorRegistry<OpenApiTag> tagImplementationGeneratorRegistry,
        IOperationNameProvider operationNameProvider)
        : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (ILocatedOpenApiElement<OpenApiTag> tag in GetTags())
            {
                SyntaxTree? tree = tagGeneratorRegistry.Get(tag).GenerateSyntaxTree();
                if (tree is not null)
                {
                    yield return tree;
                }

                tree = tagImplementationGeneratorRegistry.Get(tag).GenerateSyntaxTree();
                if (tree is not null)
                {
                    yield return tree;
                }
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags() => document.Paths.ToLocatedElements()
            .GetOperations()
            .WhereOperationHasName(operationNameProvider)
            .GetTags()
            .Distinct(TagComparer.Instance);

    }
}
