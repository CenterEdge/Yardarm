using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiMediaType>
    {
        private readonly GenerationContext _context;
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly ISerializerSelector _serializerSelector;
        private readonly IBuildContentMethodGenerator _buildContentMethodGenerator;

        public MediaTypeGeneratorFactory(GenerationContext context, IRequestsNamespace requestsNamespace,
            ISerializerSelector serializerSelector, IBuildContentMethodGenerator buildContentMethodGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _requestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));
            _serializerSelector = serializerSelector ?? throw new ArgumentNullException(nameof(serializerSelector));
            _buildContentMethodGenerator = buildContentMethodGenerator ??
                                           throw new ArgumentNullException(nameof(buildContentMethodGenerator));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiMediaType> element, ITypeGenerator? parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return parent switch
            {
                NoopTypeGenerator<OpenApiRequestBody> requestBodyParent => new RequestMediaTypeGenerator(element, _context,
                    requestBodyParent, _requestsNamespace, _serializerSelector, _buildContentMethodGenerator),
                ResponseTypeGenerator noop => new NoopTypeGenerator<OpenApiMediaType>(noop),
                _ => throw new ArgumentException("Unknown parent type for OpenApiMediaType", nameof(parent))
            };
        }
    }
}
