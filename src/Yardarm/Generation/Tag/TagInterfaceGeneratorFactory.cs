using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Api;

namespace Yardarm.Generation.Tag
{
    public class TagInterfaceGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly GenerationContext _context;

        public TagInterfaceGeneratorFactory(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiTag> element) =>
            new TagInterfaceTypeGenerator(element, _context);
    }
}
