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

    private ResponseBodyInfo(ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType,
        TypeSyntax? itemType, bool isStreaming)
    {
        MediaType = mediaType;
        BodyType = bodyType;
        ItemType = itemType;
        IsStreaming = isStreaming;
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

        return new ResponseBodyInfo(mediaType, bodyType, itemType: null, isStreaming: false);
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
    public static ResponseBodyInfo ForList(ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType,
        TypeSyntax itemType, bool isStreaming)
    {
        ArgumentNullException.ThrowIfNull(mediaType);
        ArgumentNullException.ThrowIfNull(bodyType);
        ArgumentNullException.ThrowIfNull(itemType);

        return new ResponseBodyInfo(mediaType, bodyType, itemType, isStreaming);
    }
}
