using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Yardarm.Spec;

/// <summary>
/// Converts the OpenAI additional operations extension into path item operations.
/// </summary>
internal static class OpenApiAdditionalOperationsConverter
{
    private const string ExtensionName = "x-oai-additionalOperations";

    public static void Convert(OpenApiDocument document, OpenApiSpecVersion specificationVersion,
        OpenApiReaderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(settings);

        if (specificationVersion is not (OpenApiSpecVersion.OpenApi3_0 or OpenApiSpecVersion.OpenApi3_1))
        {
            return;
        }

        var reader = new OpenApiJsonReader();
        foreach ((string path, IOpenApiPathItem pathItem) in document.Paths)
        {
            if (pathItem.Extensions?.TryGetValue(ExtensionName, out IOpenApiExtension? extension) != true)
            {
                continue;
            }

            if (extension is not JsonNodeExtension { Node: JsonObject additionalOperations })
            {
                throw new InvalidDataException(
                    $"The {ExtensionName} extension on path '{path}' must be an object.");
            }

            foreach ((string method, JsonNode? node) in additionalOperations)
            {
                if (string.IsNullOrWhiteSpace(method) || node is not JsonObject)
                {
                    throw new InvalidDataException(
                        $"Each {ExtensionName} entry on path '{path}' must have a method name and object value.");
                }

                var operation = reader.ReadFragment<OpenApiOperation>(
                    node,
                    specificationVersion,
                    document,
                    out OpenApiDiagnostic diagnostic,
                    settings);

                if (diagnostic.Errors.Count > 0)
                {
                    throw new InvalidDataException(
                        $"The {ExtensionName} operation '{method}' on path '{path}' is invalid: " +
                        diagnostic.Errors[0].Message);
                }

                if (operation is null)
                {
                    throw new InvalidDataException(
                        $"The {ExtensionName} operation '{method}' on path '{path}' could not be read.");
                }

                if (pathItem is not OpenApiPathItem mutablePathItem)
                {
                    throw new InvalidDataException(
                        $"Path '{path}' does not support additional operations.");
                }

                var operations = mutablePathItem.Operations ??= [];
                if (!operations.TryAdd(new HttpMethod(method), operation))
                {
                    throw new InvalidDataException(
                        $"The {ExtensionName} operation '{method}' on path '{path}' duplicates an existing operation.");
                }
            }
        }
    }
}
