using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestBodyTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiRequestBody>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;

        public RequestBodyTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            new RequestBodyTypeGenerator(element, _context, _mediaTypeSelector);
    }
}
