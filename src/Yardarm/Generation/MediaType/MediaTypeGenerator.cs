using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiMediaType> _mediaTypeGeneratorRegistry;
        private readonly ISerializerSelector _serializerSelector;

        public MediaTypeGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiMediaType> mediaTypeGeneratorRegistry,
            ISerializerSelector serializerSelector)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _mediaTypeGeneratorRegistry = mediaTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(mediaTypeGeneratorRegistry));
            _serializerSelector = serializerSelector ?? throw new ArgumentNullException(nameof(serializerSelector));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetMediaTypes()
                .Where(p => _serializerSelector.Select(p) != null)
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes() =>
            _document.Paths.ToLocatedElements()
                .GetOperations()
                .GetRequestBodies()
                .GetMediaTypes();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            _mediaTypeGeneratorRegistry.Get(mediaType).GenerateSyntaxTree();
    }
}
