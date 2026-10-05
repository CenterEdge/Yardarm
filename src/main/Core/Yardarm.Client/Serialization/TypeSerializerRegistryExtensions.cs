using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

public static class TypeSerializerRegistryExtensions
{
    extension(ITypeSerializerRegistry typeSerializerRegistry)
    {
        public ITypeSerializerRegistry Add(IEnumerable<string> mediaTypes, ITypeSerializer typeSerializer)
        {
            foreach (string mediaType in mediaTypes)
            {
                typeSerializerRegistry.Add(mediaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }

        public ITypeSerializerRegistry Add(IEnumerable<Type> schemaTypes, ITypeSerializer typeSerializer)
        {
            foreach (Type schemaType in schemaTypes)
            {
                typeSerializerRegistry.Add(schemaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }

        public ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            IEnumerable<string> mediaTypes)
            where T : ITypeSerializer =>
            typeSerializerRegistry.Add<T>(mediaTypes, null);

        public ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            IEnumerable<Type> schemaTypes)
            where T : ITypeSerializer =>
            typeSerializerRegistry.Add<T>(null, schemaTypes);

        internal ITypeSerializerRegistry Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
            IEnumerable<string>? mediaTypes = null,
            IEnumerable<Type>? schemaTypes = null)
            where T : ITypeSerializer
        {
            ConstructorInfo? constructor = typeof(T).GetConstructor([typeof(ITypeSerializerRegistry)]);

            ITypeSerializer serializer = (ITypeSerializer?)constructor?.Invoke([typeSerializerRegistry]) ??
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
        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData) =>
            typeSerializerRegistry.DeserializeAsync<T>(content, serializationData, default);

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            CancellationToken cancellationToken = default) =>
            GetSerializer(typeSerializerRegistry, content, typeof(T))
                .DeserializeAsync<T>(content, serializationData, cancellationToken);

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
            GetSerializer(typeSerializerRegistry, mediaType, typeof(T))
                .Serialize(value, mediaType, serializationData);

        /// <summary>
        /// Serializes a sequence of items. Collections that are already in memory, and all items when the serializer
        /// does not implement <see cref="ISequenceTypeSerializer"/>, are serialized as a <see cref="List{T}"/>.
        /// </summary>
        public HttpContent SerializeSequence<TItem>(IEnumerable<TItem> items, string mediaType,
            ISerializationData? serializationData = null)
        {
            ITypeSerializer typeSerializer = GetSerializer(typeSerializerRegistry, mediaType, typeof(List<TItem>));

            // Collections already in memory are faster to serialize synchronously than through an asynchronous sequence
            if (!SequenceHelpers.TryGetInMemoryList(items, out List<TItem>? list))
            {
                if (typeSerializer is ISequenceTypeSerializer sequenceTypeSerializer)
                {
                    return sequenceTypeSerializer.SerializeSequence(SequenceHelpers.FromEnumerable(items), mediaType,
                        serializationData);
                }

                list = [.. items];
            }

            return typeSerializer.Serialize(list, mediaType, serializationData);
        }

        /// <summary>
        /// Serializes an asynchronous sequence of items. Collections that are already in memory are serialized as a
        /// <see cref="List{T}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The items are not a collection that is already in memory, and the
        /// serializer does not implement <see cref="ISequenceTypeSerializer"/>.</exception>
        [OverloadResolutionPriority(1)]
        public HttpContent SerializeSequence<TItem>(IAsyncEnumerable<TItem> items, string mediaType,
            ISerializationData? serializationData = null)
        {
            ITypeSerializer typeSerializer = GetSerializer(typeSerializerRegistry, mediaType, typeof(List<TItem>));

            // Collections already in memory are faster to serialize synchronously, and work with any serializer
            if (SequenceHelpers.TryGetInMemoryList(items, out List<TItem>? list))
            {
                return typeSerializer.Serialize(list, mediaType, serializationData);
            }

            return typeSerializer is ISequenceTypeSerializer sequenceTypeSerializer
                ? sequenceTypeSerializer.SerializeSequence(items, mediaType, serializationData)
                : throw new NotSupportedException(
                    $"The serializer for media type '{mediaType}' does not support asynchronous sequences.");
        }

        /// <summary>
        /// Deserializes a sequence of items. If the serializer does not implement <see cref="ISequenceTypeSerializer"/>,
        /// the content is deserialized as a <see cref="List{T}"/> before the first item is returned.
        /// </summary>
        public IAsyncEnumerable<TItem> DeserializeSequenceAsync<TItem>(HttpContent content,
            ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
        {
            ITypeSerializer typeSerializer = GetSerializer(typeSerializerRegistry, content, typeof(List<TItem>));

            return typeSerializer is ISequenceTypeSerializer sequenceTypeSerializer
                ? sequenceTypeSerializer.DeserializeSequenceAsync<TItem>(content, serializationData, cancellationToken)
                : DeserializeListAsSequence<TItem>(typeSerializer, content, serializationData, cancellationToken);
        }

        /// <summary>
        /// Deserializes a sequence of items into a <see cref="List{T}"/>.
        /// </summary>
        public ValueTask<List<TItem>> DeserializeSequenceToListAsync<TItem>(HttpContent content,
            ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
        {
            ITypeSerializer typeSerializer = GetSerializer(typeSerializerRegistry, content, typeof(List<TItem>));

            return typeSerializer is ISequenceTypeSerializer sequenceTypeSerializer
                ? SequenceHelpers.ToListAsync(
                    sequenceTypeSerializer.DeserializeSequenceAsync<TItem>(content, serializationData, cancellationToken),
                    cancellationToken)
                : typeSerializer.DeserializeAsync<List<TItem>>(content, serializationData, cancellationToken);
        }
    }

    private static ITypeSerializer GetSerializer(ITypeSerializerRegistry typeSerializerRegistry, HttpContent content,
        Type schemaType) =>
        GetSerializer(typeSerializerRegistry, content.Headers.ContentType?.MediaType, schemaType, content);

    private static ITypeSerializer GetSerializer(ITypeSerializerRegistry typeSerializerRegistry, string? mediaType,
        Type schemaType, HttpContent? content = null)
    {
        if (mediaType is null || !typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? typeSerializer))
        {
            // If there is no exact match by media type, fallback to find a match by schema type
            if (!typeSerializerRegistry.TryGet(schemaType, out typeSerializer))
            {
                throw new UnknownMediaTypeException(mediaType, content);
            }
        }

        return typeSerializer;
    }

    private static async IAsyncEnumerable<TItem> DeserializeListAsSequence<TItem>(ITypeSerializer typeSerializer,
        HttpContent content, ISerializationData? serializationData,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<TItem>? items = await typeSerializer
            .DeserializeAsync<List<TItem>>(content, serializationData, cancellationToken)
            .ConfigureAwait(false);
        if (items is null)
        {
            yield break;
        }

        foreach (TItem item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }
}
