using System;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public class DefaultRequestBodyGeneratorFactory : ITypeGeneratorFactory<OpenApiRequestBody>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;

        public DefaultRequestBodyGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            new RequestBodyTypeGenerator(element, _context, _mediaTypeSelector);
    }
}
