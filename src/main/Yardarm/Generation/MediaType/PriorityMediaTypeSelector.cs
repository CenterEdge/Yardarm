using System;
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

        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiResponse> response)
        {
            ILocatedOpenApiElement<OpenApiMediaType>? highestPriorityMediaType = null;
            double? highestQuality = null;

            // Select the highest priority media type. In the event of a tie, the first one wins. In the event there
            // is no matching serializer, select the first binary string.
            foreach (var mediaType in response.GetMediaTypes())
            {
                double quality = _serializerSelector.Select(mediaType)?.Quality ?? 0.0;
                if (quality == 0 && mediaType.Element.Schema is not { Type: "string", Format: "binary" })
                {
                    // Don't allow a media type with no serializer to be selected unless it's a binary string
                    continue;
                }

                if (highestQuality is null || quality > highestQuality)
                {
                    highestQuality = quality;
                    highestPriorityMediaType = mediaType;
                }
            }

            return highestPriorityMediaType;
        }
    }
}
