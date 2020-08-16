using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiOperation>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;
        private readonly IBuildUriMethodGenerator _buildUriMethodGenerator;
        private readonly IAddHeadersMethodGenerator _addHeadersMethodGenerator;

        public RequestTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IBuildUriMethodGenerator buildUriMethodGenerator, IAddHeadersMethodGenerator addHeadersMethodGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _buildUriMethodGenerator = buildUriMethodGenerator ??
                                       throw new ArgumentNullException(nameof(buildUriMethodGenerator));
            _addHeadersMethodGenerator = addHeadersMethodGenerator ?? throw new ArgumentNullException(nameof(addHeadersMethodGenerator));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiOperation> element) =>
            new RequestTypeGenerator(element, _context, _mediaTypeSelector, _buildUriMethodGenerator,
                _addHeadersMethodGenerator);
    }
}
