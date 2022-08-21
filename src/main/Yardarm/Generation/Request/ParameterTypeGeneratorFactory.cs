using System;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class ParameterTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiParameter>
    {
        private readonly GenerationContext _context;

        public ParameterTypeGeneratorFactory(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiParameter> element, ITypeGenerator? parent) =>
            new ParameterTypeGenerator(element, _context, parent);
    }
}
