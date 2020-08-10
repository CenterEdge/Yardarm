using System;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Response
{
    public class ResponsesTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponses>
    {
        private readonly GenerationContext _context;
        private readonly ResponsesBaseTypeGenerator _responsesBaseTypeGenerator;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        public ResponsesTypeGeneratorFactory(GenerationContext context,
            ResponsesBaseTypeGenerator responsesBaseTypeGenerator,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responsesBaseTypeGenerator = responsesBaseTypeGenerator ?? throw new ArgumentNullException(nameof(responsesBaseTypeGenerator));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiResponses> element) =>
            new ResponsesTypeGenerator(element, _context, _responsesBaseTypeGenerator, _httpResponseCodeNameProvider);
    }
}
