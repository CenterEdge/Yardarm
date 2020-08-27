using System;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class UnknownResponseTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiUnknownResponse>
    {
        private readonly GenerationContext _context;
        private readonly IResponsesNamespace _responsesNamespace;
        private readonly ISerializationNamespace _serializationNamespace;

        public UnknownResponseTypeGeneratorFactory(GenerationContext context,
            IResponsesNamespace responsesNamespace,
            ISerializationNamespace serializationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responsesNamespace = responsesNamespace ??
                                  throw new ArgumentNullException(nameof(responsesNamespace));
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiUnknownResponse> element, ITypeGenerator? parent) =>
            new UnknownResponseTypeGenerator(element, _context, _serializationNamespace, _responsesNamespace);
    }
}
