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

        // A response which references a component uses the body of the component, not an operation-local copy
        response = response.GetPrimaryResponse();

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

        TypeSyntax? itemType = GetListItemType(mediaType);
        if (itemType is null)
        {
            return ResponseBodyInfo.ForNonList(mediaType, bodyType);
        }

        // Media types with an itemSchema, such as JSON Lines, always stream, arrays must opt in. The serializer must
        // also support streaming.
        bool isStreaming = (mediaType.GetItemSchema() is not null || HasStreamingExtension(mediaType))
            && serializerSelector.Select(mediaType) is { Descriptor.SupportsStreaming: true };

        return ResponseBodyInfo.ForList(
            mediaType,
            isStreaming ? WellKnownTypes.System.Collections.Generic.IAsyncEnumerableT.Name(itemType) : bodyType,
            itemType,
            isStreaming);
    }

    public IEnumerable<(ILocatedOpenApiElement<IOpenApiMediaType> MediaType, double Quality)> GetCompatibleMediaTypes(
        ILocatedOpenApiElement<IOpenApiResponse> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response = response.GetPrimaryResponse();

        var compatible = response.GetCompatibleMediaTypes(mediaTypeSelector, serializerSelector, context.TypeGeneratorRegistry);

        // Compatibility is determined by the buffered body type, but a streamed body can only be read by serializers
        // that support streaming. Another media type, such as XML, must not be requested if it can't produce the body.
        return Resolve(response) is { IsStreaming: true }
            ? compatible.Where(p => serializerSelector.Select(p.MediaType) is { Descriptor.SupportsStreaming: true })
            : compatible;
    }

    // Gets the item type of a list body, which is an array schema or a media type with an itemSchema. The item type
    // comes from the schema, not from the generated body type, so it doesn't depend on how the list is represented.
    private TypeSyntax? GetListItemType(ILocatedOpenApiElement<IOpenApiMediaType> mediaType)
    {
        if (mediaType.GetItemType(context.TypeGeneratorRegistry) is { } itemType)
        {
            return itemType;
        }

        return mediaType.GetSchema() is { } schema && schema.Element.IsType(JsonSchemaType.Array)
            ? context.TypeGeneratorRegistry.Get(schema.GetItemSchemaOrDefault()).TypeInfo.Name
            : null;
    }

    private static bool HasStreamingExtension(ILocatedOpenApiElement<IOpenApiMediaType> mediaType) =>
        mediaType.Element.Extensions?.TryGetValue(StreamingExtensionName, out IOpenApiExtension? extension) == true
        && extension is JsonNodeExtension { Node: JsonValue value }
        && value.TryGetValue<bool>(out bool streaming)
        && streaming;
}
