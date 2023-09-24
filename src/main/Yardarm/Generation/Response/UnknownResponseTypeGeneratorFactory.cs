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
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(responsesNamespace);
            ArgumentNullException.ThrowIfNull(serializationNamespace);

            _context = context;
            _responsesNamespace = responsesNamespace;
            _serializationNamespace = serializationNamespace;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiUnknownResponse> element, ITypeGenerator? parent) =>
            new UnknownResponseTypeGenerator(element, _context, _serializationNamespace, _responsesNamespace);
    }
}
