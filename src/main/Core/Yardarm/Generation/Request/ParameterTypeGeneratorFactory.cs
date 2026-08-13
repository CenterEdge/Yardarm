using System;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class ParameterTypeGeneratorFactory : ITypeGeneratorFactory<IOpenApiParameter>
    {
        private readonly GenerationContext _context;

        public ParameterTypeGeneratorFactory(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiParameter> element, ITypeGenerator? parent) =>
            new ParameterTypeGenerator(element, _context, parent);
    }
}
