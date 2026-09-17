using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Yardarm.Spec;

/// <summary>
/// Converts the OpenAPI additional operations extension into path item operations.
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

        // Build a validation rule set that excludes the path parameter rule, since additional operations will not have path
        // information as they are read within the extension.
        var operationRuleSet = ValidationRuleSet.GetDefaultRuleSet();
        operationRuleSet.Remove(OpenApiParameterRules.PathParameterShouldBeInThePath.Name);

        var operationReaderSettings = new OpenApiReaderSettings
        {
            RuleSet = operationRuleSet,
        };

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

            OpenApiPathItem? mutablePathItem = pathItem switch
            {
                OpenApiPathItem concretePathItem => concretePathItem,
                OpenApiPathItemReference { RecursiveTarget: OpenApiPathItem referencedPathItem } => referencedPathItem,
                _ => null
            };

            if (mutablePathItem is null)
            {
                throw new InvalidDataException(
                    $"Path '{path}' does not support additional operations.");
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
                    operationReaderSettings);

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

                var operations = mutablePathItem.Operations ??= [];
                if (!operations.TryAdd(new HttpMethod(method), operation))
                {
                    throw new InvalidDataException(
                        $"The {ExtensionName} operation '{method}' on path '{path}' duplicates an existing operation.");
                }
            }

            mutablePathItem.Extensions?.Remove(ExtensionName);
        }

        // Run a focused validation of path items on the document once all additional operations have been added.
        var pathParameterRuleSet = ValidationRuleSet.GetEmptyRuleSet();
        pathParameterRuleSet.Add(
            typeof(IOpenApiParameter),
            OpenApiParameterRules.PathParameterShouldBeInThePath);

        var validationErrors = document.Validate(pathParameterRuleSet).ToList();
        if (validationErrors.Count > 0)
        {
            throw new InvalidDataException(validationErrors[0].Message);
        }
    }
}
