using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class PriorityMediaTypeSelector : IMediaTypeSelector
    {
        private readonly ISerializerSelector _serializerSelector;

        public PriorityMediaTypeSelector(ISerializerSelector serializerSelector)
        {
            ArgumentNullException.ThrowIfNull(serializerSelector);

            _serializerSelector = serializerSelector;
        }

        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiResponse> response) =>
            response
                .GetMediaTypes()
                .Select(mediaType => (mediaType, descriptor: _serializerSelector.Select(mediaType)))
                .Where(p => p.descriptor != null)
                .OrderByDescending(p => p.descriptor.GetValueOrDefault().Quality)
                .Select(p => p.mediaType)
                .FirstOrDefault();
    }
}
