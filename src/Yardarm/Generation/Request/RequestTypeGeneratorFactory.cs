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
        private readonly IBuildRequestMethodGenerator _buildRequestMethodGenerator;
        private readonly IBuildUriMethodGenerator _buildUriMethodGenerator;
        private readonly IAddHeadersMethodGenerator _addHeadersMethodGenerator;
        private readonly IBuildContentMethodGenerator _buildContentMethodGenerator;

        public RequestTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IBuildRequestMethodGenerator buildRequestMethodGenerator, IBuildUriMethodGenerator buildUriMethodGenerator,
            IAddHeadersMethodGenerator addHeadersMethodGenerator, IBuildContentMethodGenerator buildContentMethodGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _buildRequestMethodGenerator = buildRequestMethodGenerator ?? throw new ArgumentNullException(nameof(buildRequestMethodGenerator));
            _buildUriMethodGenerator = buildUriMethodGenerator ??
                                       throw new ArgumentNullException(nameof(buildUriMethodGenerator));
            _addHeadersMethodGenerator = addHeadersMethodGenerator ??
                                         throw new ArgumentNullException(nameof(addHeadersMethodGenerator));
            _buildContentMethodGenerator = buildContentMethodGenerator ??
                                           throw new ArgumentNullException(nameof(buildContentMethodGenerator));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiOperation> element) =>
            new RequestTypeGenerator(element, _context, _mediaTypeSelector, _buildRequestMethodGenerator,
                _buildUriMethodGenerator, _addHeadersMethodGenerator, _buildContentMethodGenerator);
    }
}
