using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Threading;
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
                typeSerializerRegistry.Add(mediaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }

        public static ITypeSerializerRegistry Add(this ITypeSerializerRegistry typeSerializerRegistry,
            IEnumerable<Type> schemaTypes, ITypeSerializer typeSerializer)
        {
            foreach (Type schemaType in schemaTypes)
            {
                typeSerializerRegistry.Add(schemaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }

        public static ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            this ITypeSerializerRegistry typeSerializerRegistry, IEnumerable<string> mediaTypes)
            where T : ITypeSerializer =>
            typeSerializerRegistry.Add<T>(mediaTypes, null);

        public static ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            this ITypeSerializerRegistry typeSerializerRegistry, IEnumerable<Type> schemaTypes)
            where T : ITypeSerializer =>
            typeSerializerRegistry.Add<T>(null, schemaTypes);

        internal static ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            this ITypeSerializerRegistry typeSerializerRegistry,
            IEnumerable<string>? mediaTypes = null,
            IEnumerable<Type>? schemaTypes = null)
            where T : ITypeSerializer
        {
            ConstructorInfo? constructor = typeof(T).GetConstructor(new[] {typeof(ITypeSerializerRegistry)});

            ITypeSerializer serializer = (ITypeSerializer?)constructor?.Invoke(new object[] {typeSerializerRegistry}) ??
                                         Activator.CreateInstance<T>();

            if (mediaTypes is not null)
            {
                typeSerializerRegistry.Add(mediaTypes, serializer);
            }

            if (schemaTypes is not null)
            {
                typeSerializerRegistry.Add(schemaTypes, serializer);
            }

            return typeSerializerRegistry;
        }

        // Retained for backward compatibility of the public API surface in the generated SDK
        public static ValueTask<T> DeserializeAsync<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            HttpContent content, ISerializationData? serializationData) =>
            typeSerializerRegistry.DeserializeAsync<T>(content, serializationData, default);

        public static ValueTask<T> DeserializeAsync<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            HttpContent content, ISerializationData? serializationData = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            CancellationToken cancellationToken = default)
        {
            string? mediaType = content.Headers.ContentType?.MediaType;

            if (mediaType is null || !typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? typeSerializer))
            {
                // If there is no exact match by media type, fallback to find a match by schema type
                if (!typeSerializerRegistry.TryGet(typeof(T), out typeSerializer))
                {
                    throw new UnknownMediaTypeException(mediaType, content);
                }
            }

            return typeSerializer.DeserializeAsync<T>(content, serializationData, cancellationToken);
        }

        public static HttpContent Serialize<T>(this ITypeSerializerRegistry typeSerializerRegistry,
            T value, string mediaType, ISerializationData? serializationData = null)
        {
            if (!typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? typeSerializer))
            {
                // If there is no exact match by media type, fallback to find a match by schema type
                if (!typeSerializerRegistry.TryGet(typeof(T), out typeSerializer))
                {
                    throw new UnknownMediaTypeException(mediaType);
                }
            }

            return typeSerializer.Serialize(value, mediaType, serializationData);
        }
    }
}
