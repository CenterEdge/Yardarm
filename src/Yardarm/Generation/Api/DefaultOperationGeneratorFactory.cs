using System;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public class DefaultOperationGeneratorFactory : ITypeGeneratorFactory<OpenApiOperation>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;

        public DefaultOperationGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public ITypeGenerator Create(LocatedOpenApiElement<OpenApiOperation> element) =>
            new OperationTypeGenerator(element, _context, _mediaTypeSelector);
    }
}
