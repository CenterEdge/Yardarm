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

        public ResponseSetTypeGeneratorFactory(GenerationContext context,
            IResponsesNamespace responsesNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responsesNamespace = responsesNamespace ??
                                  throw new ArgumentNullException(nameof(responsesNamespace));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiResponses> element) =>
            new ResponseSetTypeGenerator(element, _context, _responsesNamespace);
    }
}
