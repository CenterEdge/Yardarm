using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class TypeSerializerRegistry : ITypeSerializerRegistry
    {
        private readonly IDictionary<string, ITypeSerializer> _registry = new Dictionary<string, ITypeSerializer>();

        public ITypeSerializer Get(string mediaType) => _registry[mediaType];

        public bool TryGet(string mediaType, [MaybeNullWhen(false)] out ITypeSerializer typeSerializer) =>
            _registry.TryGetValue(mediaType, out typeSerializer);

        public ITypeSerializerRegistry Add(string mediaType, ITypeSerializer serializer)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            _registry.Add(mediaType, serializer);

            return this;
        }

        public static ITypeSerializerRegistry CreateDefaultRegistry() =>
            new TypeSerializerRegistry()
                .Add(PlainTextSerializer.SupportedMediaTypes, new PlainTextSerializer());
    }
}
