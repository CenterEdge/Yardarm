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
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(responsesNamespace);

            _context = context;
            _responsesNamespace = responsesNamespace;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiResponses> element, ITypeGenerator? parent) =>
            new ResponseSetTypeGenerator(element, _context, _responsesNamespace);
    }
}
