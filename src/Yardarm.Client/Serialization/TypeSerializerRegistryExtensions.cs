using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public static class TypeSerializerRegistryExtensions
    {
        public static ITypeSerializerRegistry Add(this ITypeSerializerRegistry typeSerializerRegistry,
            IEnumerable<string> mediaTypes, ITypeSerializer typeSerializer)
        {
            foreach (string mediaType in mediaTypes)
            {
                typeSerializerRegistry = typeSerializerRegistry.Add(mediaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }

        public static ITypeSerializerRegistry Add<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            IEnumerable<string> mediaTypes)
            where T : ITypeSerializer
        {
            ConstructorInfo? constructor = typeof(T).GetConstructor(new[] {typeof(ITypeSerializerRegistry)});

            ITypeSerializer serializer = (ITypeSerializer?)constructor?.Invoke(new object[] {typeSerializerRegistry}) ??
                                         Activator.CreateInstance<T>();

            return typeSerializerRegistry.Add(mediaTypes, serializer);
        }

        public static ValueTask<T> DeserializeAsync<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            HttpContent content)
        {
            string mediaType = content.Headers.ContentType.MediaType;

            if (!typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? typeSerializer))
            {
                throw new UnknownMediaTypeException(mediaType, content);
            }

            return typeSerializer.DeserializeAsync<T>(content);
        }

        public static HttpContent Serialize<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            T value, string mediaType)
        {
            if (!typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? typeSerializer))
            {
                throw new UnknownMediaTypeException(mediaType);
            }

            return typeSerializer.Serialize(value, mediaType);
        }
    }
}
