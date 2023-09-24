using System;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class HeaderTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiHeader>
    {
        private readonly GenerationContext _context;

        public HeaderTypeGeneratorFactory(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiHeader> element, ITypeGenerator? parent) =>
            new HeaderTypeGenerator(element, _context, parent);
    }
}
