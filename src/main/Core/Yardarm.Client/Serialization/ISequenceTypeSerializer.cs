using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

/// <summary>
/// A <see cref="ITypeSerializer"/> that can serialize and deserialize a sequence of items, such as
/// a JSON array or JSON Lines, with the item type known at compile time.
/// </summary>
/// <remarks>
/// Implementing this interface is optional. The sequence methods on <see cref="TypeSerializerRegistryExtensions"/>
/// fall back to <see cref="ITypeSerializer"/> with a <see cref="List{T}"/> when a serializer does not implement it.
/// </remarks>
public interface ISequenceTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// Serializes a sequence of items.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="items">The items to serialize. They are enumerated when the content is sent.</param>
    /// <param name="mediaType">The media type of the content.</param>
    /// <param name="serializationData">Additional data for serialization.</param>
    /// <returns>The serialized content.</returns>
    HttpContent SerializeSequence<TItem>(IAsyncEnumerable<TItem> items, string mediaType,
        ISerializationData? serializationData = null);

    /// <summary>
    /// Deserializes a sequence of items.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="content">The content to deserialize.</param>
    /// <param name="serializationData">Additional data for deserialization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized items. The content is read as the items are enumerated.</returns>
    IAsyncEnumerable<TItem> DeserializeSequenceAsync<TItem>(HttpContent content,
        ISerializationData? serializationData = null, CancellationToken cancellationToken = default);
}
