using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponse>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;
        private readonly IResponsesNamespace _responsesNamespace;
        private readonly IGetBodyMethodGenerator _parseBodyMethodGenerator;
        private readonly ISerializationNamespace _serializationNamespace;

        public ResponseTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
            IResponsesNamespace responsesNamespace, IGetBodyMethodGenerator parseBodyMethodGenerator,
            ISerializationNamespace serializationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
            _responsesNamespace = responsesNamespace ??
                                  throw new ArgumentNullException(nameof(responsesNamespace));
            _parseBodyMethodGenerator = parseBodyMethodGenerator ??
                                        throw new ArgumentNullException(nameof(parseBodyMethodGenerator));
            _serializationNamespace = serializationNamespace ??
                                      throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiResponse> element, ITypeGenerator? parent) =>
            new ResponseTypeGenerator(element, _context, _mediaTypeSelector, _httpResponseCodeNameProvider,
                _serializationNamespace, _responsesNamespace, _parseBodyMethodGenerator);
    }
}
