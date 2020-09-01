using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class DefaultSerializerSelector : ISerializerSelector
    {
        private readonly Dictionary<string, SerializerDescriptor> _descriptors;

        public DefaultSerializerSelector(IEnumerable<SerializerDescriptor> descriptors)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            _descriptors = descriptors
                .SelectMany(
                    p => p.MediaTypes,
                    (descriptor, mediaType) => (descriptor, mediaType))
                .ToDictionary(
                    p => p.mediaType,
                    p => p.descriptor);
        }

        public SerializerDescriptor? Select(ILocatedOpenApiElement<OpenApiMediaType> mediaType)
        {
            if (_descriptors.TryGetValue(mediaType.Key, out SerializerDescriptor descriptor))
            {
                return descriptor;
            }

            return null;
        }
    }
}
