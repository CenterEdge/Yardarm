using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class TypeSerializerRegistry : ITypeSerializerRegistry
    {
        private static ITypeSerializerRegistry? s_instance;
        public static ITypeSerializerRegistry Instance
        {
            get
            {
                if (s_instance is not null)
                {
                    return s_instance;
                }

                // In case two threads are getting Instance for the first time at the same time,
                // use CompareExchange. One of the threads will not set the value, discarding the
                // CreateDefaultRegistry result, and both calls will get the same value.
                Interlocked.CompareExchange(ref s_instance, CreateDefaultRegistry(), null);
                return s_instance;
            }
            set => s_instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly Dictionary<string, ITypeSerializer> _mediaTypeRegistry = new();
        private readonly Dictionary<Type, ITypeSerializer> _schemaTypeRegistry = new();

        public ITypeSerializer Get(string mediaType)
        {
            if (mediaType is null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }

            return _mediaTypeRegistry[mediaType];
        }

        public ITypeSerializer Get(Type schemaType) => TryGet(schemaType, out ITypeSerializer? serializer)
            ? serializer
            : throw new KeyNotFoundException();

        public bool TryGet(string mediaType, [MaybeNullWhen(false)] out ITypeSerializer typeSerializer) =>
            _mediaTypeRegistry.TryGetValue(mediaType, out typeSerializer);

        public bool TryGet(Type schemaType, [MaybeNullWhen(false)] out ITypeSerializer typeSerializer)
        {
            while (true)
            {
                if (_schemaTypeRegistry.TryGetValue(schemaType, out typeSerializer))
                {
                    return true;
                }

                if (schemaType.BaseType is not null)
                {
                    // Search recursively through parent types
                    schemaType = schemaType.BaseType;
                    continue;
                }

                typeSerializer = null;
                return false;
            }
        }

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

            _mediaTypeRegistry.Add(mediaType, serializer);

            return this;
        }

        public ITypeSerializerRegistry Add(Type schemaType, ITypeSerializer serializer)
        {
            if (schemaType == null)
            {
                throw new ArgumentNullException(nameof(schemaType));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            _schemaTypeRegistry.Add(schemaType, serializer);

            return this;
        }

        public static ITypeSerializerRegistry CreateDefaultRegistry() =>
            new TypeSerializerRegistry()
                .Add<PlainTextSerializer>(PlainTextSerializer.SupportedMediaTypes)
                .Add<MultipartFormDataSerializer>(MultipartFormDataSerializer.SupportedMediaTypes)
                .Add<BinaryStreamSerializer>(BinaryStreamSerializer.SupportedMediaTypes, BinaryStreamSerializer.SupportedSchemaTypes);
    }
}
