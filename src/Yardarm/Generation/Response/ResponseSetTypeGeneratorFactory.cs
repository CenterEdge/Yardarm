using System;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseSetTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponses>
    {
        private readonly GenerationContext _context;
        private readonly IResponsesNamespace _responsesNamespace;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        public ResponseSetTypeGeneratorFactory(GenerationContext context,
            IResponsesNamespace responsesNamespace,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responsesNamespace = responsesNamespace ??
                                  throw new ArgumentNullException(nameof(responsesNamespace));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiResponses> element) =>
            new ResponseSetTypeGenerator(element, _context, _responsesNamespace, _httpResponseCodeNameProvider);
    }
}
