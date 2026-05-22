using System;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class HeaderTypeGeneratorFactory : ITypeGeneratorFactory<IOpenApiHeader>
    {
        private readonly GenerationContext _context;

        public HeaderTypeGeneratorFactory(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiHeader> element, ITypeGenerator? parent) =>
            new HeaderTypeGenerator(element, _context, parent);
    }
}
