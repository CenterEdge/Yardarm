using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiTag> _tagGeneratorRegistry;
        private readonly ITypeGeneratorRegistry<OpenApiTag, TagImplementationCategory> _tagImplementationGeneratorRegistry;

        public TagGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiTag> tagGeneratorRegistry,
            ITypeGeneratorRegistry<OpenApiTag, TagImplementationCategory> tagImplementationGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tagGeneratorRegistry);
            ArgumentNullException.ThrowIfNull(tagImplementationGeneratorRegistry);

            _document = document;
            _tagGeneratorRegistry = tagGeneratorRegistry;
            _tagImplementationGeneratorRegistry = tagImplementationGeneratorRegistry;
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (ILocatedOpenApiElement<OpenApiTag> tag in GetTags())
            {
                SyntaxTree? tree = _tagGeneratorRegistry.Get(tag).GenerateSyntaxTree();
                if (tree is not null)
                {
                    yield return tree;
                }

                tree = _tagImplementationGeneratorRegistry.Get(tag).GenerateSyntaxTree();
                if (tree is not null)
                {
                    yield return tree;
                }
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags() => _document.Paths.ToLocatedElements()
            .GetOperations()
            .GetTags()
            .Distinct(TagComparer.Instance);

    }
}
