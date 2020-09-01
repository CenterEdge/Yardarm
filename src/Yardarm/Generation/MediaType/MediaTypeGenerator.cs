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
        private readonly IMediaTypeSelector _mediaTypeSelector;

        public MediaTypeGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiMediaType> mediaTypeGeneratorRegistry,
            IMediaTypeSelector mediaTypeSelector)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _mediaTypeGeneratorRegistry = mediaTypeGeneratorRegistry ?? throw new ArgumentNullException(nameof(mediaTypeGeneratorRegistry));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetRequestBodies()
                .Select(_mediaTypeSelector.Select)
                .Where(p => p != null)
                .Select(Generate!)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies() =>
            _document.Paths.ToLocatedElements()
                .GetOperations()
                .GetRequestBodies();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            _mediaTypeGeneratorRegistry.Get(mediaType).GenerateSyntaxTree();
    }
}
