using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response;

/// <summary>
/// Describes the body of a response, as selected for deserialization. Create instances with
/// <see cref="ForList"/> or <see cref="ForNonList"/>.
/// </summary>
public sealed class ResponseBodyInfo
{
    /// <summary>
    /// The selected media type.
    /// </summary>
    public ILocatedOpenApiElement<IOpenApiMediaType> MediaType { get; }

    /// <summary>
    /// The C# type of the body. For a streamed body this is <c>IAsyncEnumerable&lt;T&gt;</c> of <see cref="ItemType"/>.
    /// </summary>
    public TypeSyntax BodyType { get; }

    /// <summary>
    /// The C# type of each item of a list body, either an array schema or a media type with an <c>itemSchema</c>,
    /// otherwise <c>null</c>.
    /// </summary>
    public TypeSyntax? ItemType { get; }

    /// <summary>
    /// If true, the body is a list, so <see cref="ItemType"/> is known.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ItemType))]
    public bool IsList => ItemType is not null;

    /// <summary>
    /// If true, the body is an <c>IAsyncEnumerable&lt;T&gt;</c> of <see cref="ItemType"/> which is read as it is
    /// enumerated. Only list bodies may be streamed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ItemType))]
    public bool IsStreaming { get; }

    /// <summary>
    /// If true, the body is deserialized with <c>DeserializeSequenceAsync</c> using <see cref="ItemType"/>, rather
    /// than as a single value. This is the case for streamed bodies, and for list bodies where a sequential media type,
    /// such as JSON Lines, may be received in place of the selected media type.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ItemType))]
    public bool DeserializeAsSequence { get; }

    private ResponseBodyInfo(ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType,
        TypeSyntax? itemType, bool isStreaming, bool deserializeAsSequence)
    {
        MediaType = mediaType;
        BodyType = bodyType;
        ItemType = itemType;
        IsStreaming = isStreaming;
        DeserializeAsSequence = deserializeAsSequence;
    }

    /// <summary>
    /// Creates the body of a response which is not a list.
    /// </summary>
    /// <param name="mediaType">The selected media type.</param>
    /// <param name="bodyType">The C# type of the body.</param>
    public static ResponseBodyInfo ForNonList(ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType)
    {
        ArgumentNullException.ThrowIfNull(mediaType);
        ArgumentNullException.ThrowIfNull(bodyType);

        return new ResponseBodyInfo(mediaType, bodyType, itemType: null, isStreaming: false, deserializeAsSequence: false);
    }

    /// <summary>
    /// Creates the body of a response which is a list.
    /// </summary>
    /// <param name="mediaType">The selected media type.</param>
    /// <param name="bodyType">
    /// The C# type of the body. This should be <c>IAsyncEnumerable&lt;T&gt;</c> of <paramref name="itemType"/> if
    /// <paramref name="isStreaming"/> is true.
    /// </param>
    /// <param name="itemType">The C# type of each item.</param>
    /// <param name="isStreaming">If true, the body is streamed.</param>
    /// <param name="hasSequentialMediaType">
    /// If true, a sequential media type, such as JSON Lines, may be received for the response, so the body must be
    /// deserialized as a sequence even when it isn't streamed.
    /// </param>
    public static ResponseBodyInfo ForList(ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType,
        TypeSyntax itemType, bool isStreaming, bool hasSequentialMediaType)
    {
        ArgumentNullException.ThrowIfNull(mediaType);
        ArgumentNullException.ThrowIfNull(bodyType);
        ArgumentNullException.ThrowIfNull(itemType);

        return new ResponseBodyInfo(mediaType, bodyType, itemType, isStreaming,
            deserializeAsSequence: isStreaming || hasSequentialMediaType);
    }
}
