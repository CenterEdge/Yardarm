using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Yardarm.Client.Internal;

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
            set
            {
                ThrowHelper.ThrowIfNull(value);

                s_instance = value;
            }
        }

        private readonly Dictionary<string, ITypeSerializer> _mediaTypeRegistry = new();
        private readonly Dictionary<Type, ITypeSerializer> _schemaTypeRegistry = new();

        public ITypeSerializer Get(string mediaType)
        {
            ThrowHelper.ThrowIfNull(mediaType);

            return _mediaTypeRegistry[mediaType];
        }

        public ITypeSerializer Get(Type schemaType)
        {
            if (TryGet(schemaType, out ITypeSerializer? serializer))
            {
                return serializer;
            }

            ThrowHelper.ThrowKeyNotFoundException();
            return null!;
        }

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
            ThrowHelper.ThrowIfNull(mediaType);
            ThrowHelper.ThrowIfNull(serializer);

            _mediaTypeRegistry.Add(mediaType, serializer);

            return this;
        }

        public ITypeSerializerRegistry Add(Type schemaType, ITypeSerializer serializer)
        {
            ThrowHelper.ThrowIfNull(schemaType);
            ThrowHelper.ThrowIfNull(serializer);

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
