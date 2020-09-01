using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiMediaType>
    {
        private readonly GenerationContext _context;
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly IBuildContentMethodGenerator _buildContentMethodGenerator;

        public MediaTypeGeneratorFactory(GenerationContext context, IRequestsNamespace requestsNamespace,
            IBuildContentMethodGenerator buildContentMethodGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _requestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));
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
                    requestBodyParent, _requestsNamespace, _buildContentMethodGenerator),
                ResponseTypeGenerator noop => new NoopTypeGenerator<OpenApiMediaType>(noop),
                _ => throw new ArgumentException("Unknown parent type for OpenApiMediaType", nameof(parent))
            };
        }
    }
}
