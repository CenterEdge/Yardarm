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
        private readonly ResponseBaseTypeGenerator _responseBaseTypeGenerator;

        public ResponseTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
            ResponseBaseTypeGenerator responseBaseTypeGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
            _responseBaseTypeGenerator = responseBaseTypeGenerator ??
                                         throw new ArgumentNullException(nameof(responseBaseTypeGenerator));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiResponse> element) =>
            new ResponseTypeGenerator(element, _context, _mediaTypeSelector, _httpResponseCodeNameProvider, _responseBaseTypeGenerator);
    }
}
