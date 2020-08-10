using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;

namespace Yardarm.Generation.Operation
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
