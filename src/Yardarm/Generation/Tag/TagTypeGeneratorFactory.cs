using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly GenerationContext _context;
        private readonly IOperationMethodGenerator _operationMethodGenerator;
        private readonly ISerializationNamespace _serializationNamespace;

        public TagTypeGeneratorFactory(GenerationContext context, IOperationMethodGenerator operationMethodGenerator,
            ISerializationNamespace serializationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _operationMethodGenerator = operationMethodGenerator ??
                                        throw new ArgumentNullException(nameof(operationMethodGenerator));
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiTag> element) =>
            new TagTypeGenerator(element, _context, _serializationNamespace, _operationMethodGenerator);
    }
}
