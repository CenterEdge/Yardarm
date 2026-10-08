using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.Response;

/// <summary>
/// Default <see cref="IResponseBodyResolver"/>.
/// </summary>
/// <remarks>
/// A body is streamed as an <c>IAsyncEnumerable&lt;T&gt;</c> when the selected serializer supports streaming and either
/// the media type has an <c>itemSchema</c>, such as JSON Lines, or the body is an array schema and the media type has
/// <c>x-yardarm-streaming: true</c>.
/// </remarks>
internal class DefaultResponseBodyResolver(
    IMediaTypeSelector mediaTypeSelector,
    ISerializerSelector serializerSelector,
    GenerationContext context) : IResponseBodyResolver
{
    public const string StreamingExtensionName = "x-yardarm-streaming";

    public ResponseBodyInfo? Resolve(ILocatedOpenApiElement<IOpenApiResponse> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        ILocatedOpenApiElement<IOpenApiMediaType>? mediaType = mediaTypeSelector.Select(response);
        if (mediaType is null)
        {
            return null;
        }

        TypeSyntax? bodyType = mediaType.GetBodyType(context.TypeGeneratorRegistry);
        if (bodyType is null)
        {
            return null;
        }

        // Includes compatible media types which aren't selected, since the serializer is chosen by the Content-Type of the
        // received response and the Accept header may request a sequential media type
        TypeSyntax? sequenceItemType = response.GetSequenceItemType(mediaTypeSelector, serializerSelector,
            context.TypeGeneratorRegistry);

        if (serializerSelector.Select(mediaType) is not { Descriptor.SupportsStreaming: true })
        {
            return new ResponseBodyInfo(mediaType, bodyType, sequenceItemType, IsStreaming: false);
        }

        // Media types with an itemSchema always stream, arrays must opt in
        TypeSyntax? itemType = mediaType.GetItemType(context.TypeGeneratorRegistry)
            ?? GetStreamingArrayItemType(mediaType, bodyType);
        if (itemType is null)
        {
            return new ResponseBodyInfo(mediaType, bodyType, sequenceItemType, IsStreaming: false);
        }

        return new ResponseBodyInfo(mediaType, WellKnownTypes.System.Collections.Generic.IAsyncEnumerableT.Name(itemType),
            itemType, IsStreaming: true);
    }

    public IEnumerable<(ILocatedOpenApiElement<IOpenApiMediaType> MediaType, double Quality)> GetCompatibleMediaTypes(
        ILocatedOpenApiElement<IOpenApiResponse> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var compatible = response.GetCompatibleMediaTypes(mediaTypeSelector, serializerSelector, context.TypeGeneratorRegistry);

        // Compatibility is determined by the buffered body type, but a streamed body can only be read by serializers
        // that support streaming. Another media type, such as XML, must not be requested if it can't produce the body.
        return Resolve(response) is { IsStreaming: true }
            ? compatible.Where(p => serializerSelector.Select(p.MediaType) is { Descriptor.SupportsStreaming: true })
            : compatible;
    }

    private TypeSyntax? GetStreamingArrayItemType(
        ILocatedOpenApiElement<IOpenApiMediaType> mediaType, TypeSyntax bodyType)
    {
        if (!HasStreamingExtension(mediaType)
            || mediaType.GetSchema() is not { } schema
            || !schema.Element.IsType(JsonSchemaType.Array))
        {
            return null;
        }

        // Ensure the body is generated as a list of the item type, so that the items are known
        TypeSyntax itemType = context.TypeGeneratorRegistry.Get(schema.GetItemSchemaOrDefault()).TypeInfo.Name;

        return WellKnownTypes.System.Collections.Generic.ListT.Name(itemType).IsEquivalentTo(bodyType)
            ? itemType
            : null;
    }

    private static bool HasStreamingExtension(ILocatedOpenApiElement<IOpenApiMediaType> mediaType) =>
        mediaType.Element.Extensions?.TryGetValue(StreamingExtensionName, out IOpenApiExtension? extension) == true
        && extension is JsonNodeExtension { Node: JsonValue value }
        && value.TryGetValue<bool>(out bool streaming)
        && streaming;
}
