using System;
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

        foreach (IOpenApiPathItem pathItem in document.Paths.Values)
        {
            var operations = pathItem switch
            {
                OpenApiPathItem concretePathItem => concretePathItem.Operations,
                OpenApiPathItemReference { RecursiveTarget: OpenApiPathItem referencedPathItem } =>
                    referencedPathItem.Operations,
                _ => null
            };

            if (operations is null)
            {
                continue;
            }

            foreach (OpenApiOperation operation in operations.Values)
            {
                ConvertResponseItemSchemas(
                    operation.Responses,
                    specificationVersion,
                    document,
                    reader);
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
