using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
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
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly ISerializerSelector _serializerSelector;

        public RequestTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IBuildRequestMethodGenerator buildRequestMethodGenerator, IBuildUriMethodGenerator buildUriMethodGenerator,
            IAddHeadersMethodGenerator addHeadersMethodGenerator, IBuildContentMethodGenerator buildContentMethodGenerator,
            IRequestsNamespace requestsNamespace, ISerializerSelector serializerSelector)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _buildRequestMethodGenerator = buildRequestMethodGenerator ??
                                           throw new ArgumentNullException(nameof(buildRequestMethodGenerator));
            _buildUriMethodGenerator = buildUriMethodGenerator ??
                                       throw new ArgumentNullException(nameof(buildUriMethodGenerator));
            _addHeadersMethodGenerator = addHeadersMethodGenerator ??
                                         throw new ArgumentNullException(nameof(addHeadersMethodGenerator));
            _buildContentMethodGenerator = buildContentMethodGenerator ??
                                           throw new ArgumentNullException(nameof(buildContentMethodGenerator));
            _requestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));
            _serializerSelector = serializerSelector ?? throw new ArgumentNullException(nameof(serializerSelector));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiOperation> element, ITypeGenerator? parent) =>
            new RequestTypeGenerator(element, _context, _mediaTypeSelector, _buildRequestMethodGenerator,
                _buildUriMethodGenerator, _addHeadersMethodGenerator, _buildContentMethodGenerator,
                _requestsNamespace, _serializerSelector);
    }
}
