using System;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Tag
{
    public class TagInterfaceTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly GenerationContext _context;

        public TagInterfaceTypeGeneratorFactory(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiTag> element) =>
            new TagInterfaceTypeGenerator(element, _context);
    }
}
