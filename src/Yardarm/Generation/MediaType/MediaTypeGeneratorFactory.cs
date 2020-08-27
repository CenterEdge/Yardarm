using System;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiMediaType>
    {
        private readonly GenerationContext _context;

        public MediaTypeGeneratorFactory(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiMediaType> element)
        {
            return new MediaTypeGenerator(element, _context);
        }
    }
}
