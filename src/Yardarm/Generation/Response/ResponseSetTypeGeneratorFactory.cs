using System;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseSetTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponses>
    {
        private readonly GenerationContext _context;
        private readonly ResponseBaseInterfaceTypeGenerator _responseBaseInterfaceTypeGenerator;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        public ResponseSetTypeGeneratorFactory(GenerationContext context,
            ResponseBaseInterfaceTypeGenerator responseBaseInterfaceTypeGenerator,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responseBaseInterfaceTypeGenerator = responseBaseInterfaceTypeGenerator ??
                                                  throw new ArgumentNullException(
                                                      nameof(responseBaseInterfaceTypeGenerator));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiResponses> element) =>
            new ResponseSetTypeGenerator(element, _context, _responseBaseInterfaceTypeGenerator, _httpResponseCodeNameProvider);
    }
}
