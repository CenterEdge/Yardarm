using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Yardarm.Spec;

/// <summary>
/// Converts downlevel JSON Lines item schema extensions into media type item schemas.
/// </summary>
internal static class OpenApiItemSchemaConverter
{
    private const string ExtensionName = "x-oai-itemSchema";

    public static void Convert(OpenApiDocument document, OpenApiSpecVersion specificationVersion)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (specificationVersion is not (OpenApiSpecVersion.OpenApi3_0 or OpenApiSpecVersion.OpenApi3_1))
        {
            return;
        }

        var reader = new OpenApiJsonReader();
        var visitedPathItems = new HashSet<OpenApiPathItem>(ReferenceEqualityComparer.Instance);

        foreach (IOpenApiPathItem pathItem in document.Paths.Values)
        {
            ConvertPathItem(pathItem, specificationVersion, document, reader, visitedPathItems);
        }

        if (document.Webhooks is { } webhooks)
        {
            foreach (IOpenApiPathItem pathItem in webhooks.Values)
            {
                ConvertPathItem(pathItem, specificationVersion, document, reader, visitedPathItems);
            }
        }

        if (document.Components?.PathItems is { } componentPathItems)
        {
            foreach (IOpenApiPathItem pathItem in componentPathItems.Values)
            {
                ConvertPathItem(pathItem, specificationVersion, document, reader, visitedPathItems);
            }
        }

        if (document.Components?.Responses is { } responses)
        {
            foreach (IOpenApiResponse response in responses.Values)
            {
                ConvertResponseItemSchema(
                    response,
                    specificationVersion,
                    document,
                    reader);
            }
        }

        if (document.Components?.RequestBodies is { } requestBodies)
        {
            foreach (IOpenApiRequestBody requestBody in requestBodies.Values)
            {
                ConvertRequestBodyItemSchema(
                    requestBody,
                    specificationVersion,
                    document,
                    reader);
            }
        }
    }

    private static void ConvertPathItem(
        IOpenApiPathItem pathItem,
        OpenApiSpecVersion specificationVersion,
        OpenApiDocument document,
        OpenApiJsonReader reader,
        HashSet<OpenApiPathItem> visitedPathItems)
    {
        OpenApiPathItem? mutablePathItem = pathItem switch
        {
            OpenApiPathItem concretePathItem => concretePathItem,
            OpenApiPathItemReference { RecursiveTarget: OpenApiPathItem referencedPathItem } => referencedPathItem,
            _ => null
        };

        if (mutablePathItem?.Operations is not { } operations || !visitedPathItems.Add(mutablePathItem))
        {
            return;
        }

        foreach (OpenApiOperation operation in operations.Values)
        {
            if (operation.RequestBody is { } requestBody)
            {
                ConvertRequestBodyItemSchema(
                    requestBody,
                    specificationVersion,
                    document,
                    reader);
            }

            ConvertResponseItemSchemas(
                operation.Responses,
                specificationVersion,
                document,
                reader);

            if (operation.Callbacks is not { } callbacks)
            {
                continue;
            }

            foreach (IOpenApiCallback callback in callbacks.Values)
            {
                if (callback.PathItems is not { } callbackPathItems)
                {
                    continue;
                }

                foreach (IOpenApiPathItem callbackPathItem in callbackPathItems.Values)
                {
                    ConvertPathItem(
                        callbackPathItem,
                        specificationVersion,
                        document,
                        reader,
                        visitedPathItems);
                }
            }
        }
    }

    private static void ConvertResponseItemSchemas(
        OpenApiResponses? responses,
        OpenApiSpecVersion specificationVersion,
        OpenApiDocument document,
        OpenApiJsonReader reader)
    {
        if (responses is null)
        {
            return;
        }

        foreach (IOpenApiResponse response in responses.Values)
        {
            ConvertResponseItemSchema(
                response,
                specificationVersion,
                document,
                reader);
        }
    }

    private static void ConvertResponseItemSchema(
        IOpenApiResponse response,
        OpenApiSpecVersion specificationVersion,
        OpenApiDocument document,
        OpenApiJsonReader reader)
    {
        if (response is not OpenApiResponse { Content: { } content })
        {
            return;
        }

        foreach (IOpenApiMediaType mediaType in content.Values)
        {
            ConvertMediaTypeItemSchema(
                mediaType,
                specificationVersion,
                document,
                reader);
        }
    }

    private static void ConvertRequestBodyItemSchema(
        IOpenApiRequestBody requestBody,
        OpenApiSpecVersion specificationVersion,
        OpenApiDocument document,
        OpenApiJsonReader reader)
    {
        if (requestBody.Content is not { } content)
        {
            return;
        }

        foreach (IOpenApiMediaType mediaType in content.Values)
        {
            ConvertMediaTypeItemSchema(
                mediaType,
                specificationVersion,
                document,
                reader);
        }
    }

    private static void ConvertMediaTypeItemSchema(
        IOpenApiMediaType mediaType,
        OpenApiSpecVersion specificationVersion,
        OpenApiDocument document,
        OpenApiJsonReader reader)
    {
        if (mediaType.Extensions?.TryGetValue(ExtensionName, out IOpenApiExtension? extension) != true)
        {
            return;
        }

        if (extension is not JsonNodeExtension { Node: JsonObject itemSchemaNode })
        {
            return;
        }

        OpenApiMediaType? mutableMediaType = mediaType switch
        {
            OpenApiMediaType concreteMediaType => concreteMediaType,
            OpenApiMediaTypeReference { RecursiveTarget: OpenApiMediaType referencedMediaType } => referencedMediaType,
            _ => null
        };

        if (mutableMediaType is null)
        {
            return;
        }

        var itemSchema = reader.ReadFragment<OpenApiSchema>(
            itemSchemaNode,
            specificationVersion,
            document,
            out OpenApiDiagnostic diagnostic);

        if (diagnostic.Errors.Count > 0 || itemSchema is null)
        {
            return;
        }

        mutableMediaType.ItemSchema = itemSchema;
        mutableMediaType.Extensions?.Remove(ExtensionName);
    }
}
