using System;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly GenerationContext _context;

        public TagTypeGeneratorFactory(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiTag> element) =>
            new TagTypeGenerator(element, _context);
    }
}
