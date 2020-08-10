using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Tag
{
    public class TagGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiTag> _tagGeneratorRegistry;

        public TagGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiTag> tagGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tagGeneratorRegistry = tagGeneratorRegistry ?? throw new ArgumentNullException(nameof(tagGeneratorRegistry));
        }

        public void Preprocess()
        {
            foreach (var tagGenerator in GetTags()
                .Select(_tagGeneratorRegistry.Get))
            {
                tagGenerator.Preprocess();
            }
        }

        public IEnumerable<SyntaxTree> Generate() => GetTags()
            .Select(p => _tagGeneratorRegistry.Get(p).GenerateSyntaxTree()!)
            .Where(p => p != null);

        private IEnumerable<LocatedOpenApiElement<OpenApiTag>> GetTags() => _document.Paths.ToLocatedElements()
            .GetOperations()
            .GetTags()
            .Distinct(new TagComparer());

        private class TagComparer : IEqualityComparer<LocatedOpenApiElement<OpenApiTag>>
        {
            public bool Equals(LocatedOpenApiElement<OpenApiTag>? x, LocatedOpenApiElement<OpenApiTag>? y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                return x.Element.Name == y.Element.Name;
            }

            public int GetHashCode(LocatedOpenApiElement<OpenApiTag> obj) => obj.Element.Name.GetHashCode();
        }
    }
}
