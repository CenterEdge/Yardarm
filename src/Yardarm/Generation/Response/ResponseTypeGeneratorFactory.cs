using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;

namespace Yardarm.Generation.Response
{
    public class ResponseTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponse>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        public ResponseTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiResponse> element) =>
            new ResponseTypeGenerator(element, _context, _mediaTypeSelector, _httpResponseCodeNameProvider);
    }
}
