using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

public interface ITypeSerializer
{
    HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null);

    // Retained for backward compatibility of the public API surface in the generated SDK. The overload
    // with a CancellationToken simply forwards to this implementation by default if not explicitly implemented.
    // Implementations which support cancellation should implement the overload with a CancellationToken and
    // forward calls from this method to that overload. Note that .NET Standard 2.0 does not support default
    // interface implementations, so this is a breaking change for SDKs targeting .NET Standard 2.0 which must
    // implement both variants.
    ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData);

    ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        CancellationToken cancellationToken = default)
#if NETCOREAPP3_1_OR_GREATER
    {
        // We can only provide a default implementation for .NET 6 and later
        return DeserializeAsync<T>(content, serializationData);
    }
#else
    ;
#endif

    // The sequence methods allow serializers to handle each element of a sequence individually, with the element
    // type known at compile time, such as for JSON Lines. By default they forward to Serialize and DeserializeAsync
    // with the sequence type. Note that .NET Standard 2.0 does not support default interface implementations, so
    // SDKs targeting .NET Standard 2.0 must implement these methods.

    /// <summary>
    /// Serializes a sequence of elements.
    /// </summary>
    /// <typeparam name="TSequence">The type of the sequence.</typeparam>
    /// <typeparam name="TElement">The type of each element of the sequence.</typeparam>
    /// <param name="value">The sequence to serialize.</param>
    /// <param name="mediaType">The media type of the content.</param>
    /// <param name="serializationData">Additional data for serialization.</param>
    /// <returns>The serialized content.</returns>
    HttpContent SerializeSequence<TSequence, TElement>(TSequence value, string mediaType,
        ISerializationData? serializationData = null)
        where TSequence : IEnumerable<TElement>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    {
        return Serialize(value, mediaType, serializationData);
    }
#else
    ;
#endif

    /// <summary>
    /// Deserializes a sequence of elements.
    /// </summary>
    /// <typeparam name="TSequence">The type of the sequence.</typeparam>
    /// <typeparam name="TElement">The type of each element of the sequence.</typeparam>
    /// <param name="content">The content to deserialize.</param>
    /// <param name="serializationData">Additional data for deserialization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized sequence.</returns>
    ValueTask<TSequence> DeserializeSequenceAsync<TSequence, TElement>(HttpContent content,
        ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
        where TSequence : IEnumerable<TElement>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    {
        return DeserializeAsync<TSequence>(content, serializationData, cancellationToken);
    }
#else
    ;
#endif
}
