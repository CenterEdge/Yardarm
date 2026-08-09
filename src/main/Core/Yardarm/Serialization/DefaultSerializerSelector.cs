using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Serialization
{
    public class DefaultSerializerSelector : ISerializerSelector
    {
        private readonly Dictionary<string, SerializerDescriptorWithPriority> _descriptors;

        public DefaultSerializerSelector(IEnumerable<SerializerDescriptor> descriptors)
        {
            ArgumentNullException.ThrowIfNull(descriptors);

            _descriptors = descriptors
                .SelectMany(
                    p => p.MediaTypes,
                    (descriptor, mediaType) => (descriptor, mediaType))
                .ToDictionary(
                    p => p.mediaType.MediaType,
                    p => new SerializerDescriptorWithPriority
                    {
                        Descriptor = p.descriptor, Quality = p.mediaType.Quality
                    });
        }

        public SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<OpenApiMediaType> mediaType)
        {
            if (_descriptors.TryGetValue(mediaType.Key, out SerializerDescriptorWithPriority descriptor))
            {
                return descriptor;
            }

            return null;
        }
    }
}
