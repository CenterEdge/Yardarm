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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiHeader> element, ITypeGenerator? parent) =>
            new HeaderTypeGenerator(element, _context, parent);
    }
}
