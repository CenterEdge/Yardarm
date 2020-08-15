using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly GenerationContext _context;
        private readonly IOperationMethodGenerator _operationMethodGenerator;

        public TagTypeGeneratorFactory(GenerationContext context, IOperationMethodGenerator operationMethodGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _operationMethodGenerator = operationMethodGenerator ?? throw new ArgumentNullException(nameof(operationMethodGenerator));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiTag> element) =>
            new TagTypeGenerator(element, _context, _operationMethodGenerator);
    }
}
