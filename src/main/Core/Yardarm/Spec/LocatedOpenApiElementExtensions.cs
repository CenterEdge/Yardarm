using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Generation.Operation;

namespace Yardarm.Spec;

public static class LocatedOpenApiElementExtensions
{
    private static readonly ConditionalWeakTable<OpenApiResponses, OpenApiUnknownResponse> _unknownResponses = [];

    private static readonly OpenApiSchema _defaultSchema = new();

    extension(ILocatedOpenApiElement element)
    {
        public bool IsRoot => element.Parent is null;

        public ILocatedOpenApiElement<T> CreateChild<T>(T child, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(child, key, element);

        public IEnumerable<ILocatedOpenApiElement> Parents()
        {
            var current = element;
            while (current.Parent != null)
            {
                current = current.Parent;
                yield return current;
            }
        }
    }

    extension<T>(ILocatedOpenApiElement<T> element)
        where T : IOpenApiReferenceable
    {
        public bool IsReference => element.Element is IOpenApiReferenceHolder;
    }

    extension<T>(T rootItem)
        where T : IOpenApiElement
    {
        public ILocatedOpenApiElement<T> CreateRoot(string key) =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);
    }

    extension<T>(IEnumerable<KeyValuePair<string, T>> rootItems)
        where T : IOpenApiElement
    {
        public IEnumerable<ILocatedOpenApiElement<T>> CreateRoot() =>
            rootItems?.Select(p => p.Value.CreateRoot(p.Key))
            ?? [];
    }

    extension(OpenApiDocument document)
    {
        // These methods collect all schemas directly owned by a given object (not a reference), including recursive
        // lookups within schemas.

        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas() =>
            (document.Components?.Schemas?.CreateRoot().SelectMany(p => p.GetAllSchemas()) ?? [])
                .Concat(document.Paths.CreateRoot().GetAllSchemas())
                .Concat(document.Components?.RequestBodies?.CreateRoot().GetAllSchemas() ?? [])
                .Concat(document.Components?.Responses?.CreateRoot().GetAllSchemas() ?? []);

        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            (document.Components?.Schemas?.CreateRoot().SelectMany(p => p.GetAllSchemas()) ?? [])
                .Concat(document.Paths.CreateRoot().GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider))
                .Concat(document.Components?.RequestBodies?.CreateRoot().GetAllSchemas() ?? [])
                .Concat(document.Components?.Responses?.CreateRoot().GetAllSchemas() ?? []);
    }

    extension(IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> tags)
    {
        internal IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> FilterDocumentationTags(OpenApiDocument document)
        {
            IEnumerable<OpenApiTag> topLevelTags = document.Tags ?? Enumerable.Empty<OpenApiTag>();
            bool hasNavigationTags = topLevelTags.Any(p => p.Kind == "nav");

            return tags.Where(tag =>
            {
                string? kind = topLevelTags.FirstOrDefault(p => p.Name == tag.Element.Name)?.Kind;

                return kind is not "badge" and not "audience"
                    && (!hasNavigationTags || kind == "nav");
            });
        }
    }



    public static IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
        operations.SelectMany(p => p.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas(
        this ILocatedOpenApiElement<OpenApiOperation> operation)
    {
        var requestBody = operation.GetRequestBody();
        if (requestBody is not null && !requestBody.IsReference)
        {
            var requestSchemas = requestBody
                .GetMediaTypes()
                .Select(p => p.GetSchema())
                .Where(p => p is not null && !p.IsReference)
                .SelectMany(p => p!.GetAllSchemas());

            foreach (var schema in requestSchemas)
            {
                yield return schema;
            }
        }

        foreach (var responseSchema in operation
                     .GetResponseSet()
                     .GetResponses()
                     .Where(p => !p.IsReference)
                     .GetAllSchemas())
        {
            yield return responseSchema;
        }
    }

    public static IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<IOpenApiRequestBody>> requestBody) =>
        requestBody.GetMediaTypes()
            .Select(p => p.GetSchema())
            .Where(p => p is not null && !p.IsReference)!
            .SelectMany(p => p!.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<IOpenApiResponse>> requestBody) =>
        requestBody.GetMediaTypes()
            .Select(p => p.GetSchema())
            .Where(p => p is not null && !p.IsReference)!
            .SelectMany(p => p!.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas(
        this ILocatedOpenApiElement<IOpenApiSchema> schema)
    {
        yield return schema;

        var itemSchema = schema.GetItemSchema();
        if (itemSchema is not null && !itemSchema.IsReference)
        {
            foreach (var childSchema in itemSchema.GetAllSchemas())
            {
                yield return childSchema;
            }
        }

        foreach (var childSchema in schema.GetProperties()
                     .Where(p => !p.IsReference)
                     .SelectMany(p => p.GetAllSchemas()))
        {
            yield return childSchema;
        }
    }

    #region PathItem

    extension(OpenApiPaths paths)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiPathItem>> ToLocatedElements() =>
            paths.Select(p => p.Value.CreateRoot(p.Key));
    }

    extension(IEnumerable<ILocatedOpenApiElement<IOpenApiPathItem>> pathItems)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas() =>
            pathItems.SelectMany(GetAllSchemas);

        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            pathItems.SelectMany(p => p.GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider));

        public IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            pathItems.SelectMany(GetOperations);

        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetParameters() =>
            pathItems.SelectMany(GetParameters);
    }

    extension(ILocatedOpenApiElement<IOpenApiPathItem> pathItem)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemas() =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().GetAllSchemas());

        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().WhereOperationHasName(operationNameProvider).GetAllSchemas());

        public IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            pathItem.Element.Operations?
                .Select(operation => pathItem.CreateChild(operation.Value, operation.Key.ToString()))
            ?? [];

        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetParameters() =>
            pathItem.Element.Parameters?
                .Select(p => pathItem.CreateChild(p, p.Name ?? string.Empty))
            ?? [];
    }

    #endregion

    #region Operation

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> WhereOperationHasName(
            IOperationNameProvider operationNameProvider) =>
            operations
                .Where(operation => !string.IsNullOrEmpty(operationNameProvider.GetOperationName(operation)));

        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetParameters() =>
            operations.SelectMany(GetParameters);

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetAllParameters() =>
            operations.SelectMany(GetAllParameters);

        public IEnumerable<ILocatedOpenApiElement<IOpenApiRequestBody>> GetRequestBodies() =>
            operations
                .Select(GetRequestBody)
                .Where(p => p != null)!;

        public IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets() =>
            operations
                .Select(GetResponseSet);

        public IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> GetTags() =>
            operations
                .SelectMany(GetTags);
    }

    extension(ILocatedOpenApiElement<OpenApiOperation> operation)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetParameters() =>
            operation.Element.Parameters?
                .Select(p => operation.CreateChild(p, p.Name ?? string.Empty))
            ?? [];

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public IEnumerable<ILocatedOpenApiElement<IOpenApiParameter>> GetAllParameters()
        {
            var parameters = operation.GetParameters();

            if (operation.Parent is ILocatedOpenApiElement<IOpenApiPathItem> { Element.Parameters.Count: > 0 } pathItem)
            {
                // Note that DistinctBy returns the first encountered match, so the fact that operation
                // parameters are first means they will be returned in favor of path parameters
                parameters = parameters
                    .Concat(pathItem.GetParameters())
                    .DistinctBy(p => p.Key, StringComparer.Ordinal);
            }

            return parameters;
        }

        public ILocatedOpenApiElement<IOpenApiRequestBody>? GetRequestBody() =>
            operation.Element.RequestBody != null
                ? operation.CreateChild(operation.Element.RequestBody, "requestBody")
                : null;

        public ILocatedOpenApiElement<OpenApiResponses> GetResponseSet() =>
            operation.CreateChild(operation.Element.Responses ?? [], "responses");

        public IEnumerable<ILocatedOpenApiElement<OpenApiSecurityRequirement>> GetSecurityRequirements() =>
            operation.Element.Security?
                .Select((requirement, index) => operation.CreateChild(requirement, index.ToString()))
            ?? [];

        public IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> GetTags() =>
            operation.Element.Tags?
                .Select((tag, index) => operation.CreateChild<IOpenApiTag>(tag, index.ToString()))
            ?? [];
    }

    #endregion

    #region Request

    extension(IEnumerable<ILocatedOpenApiElement<IOpenApiRequestBody>> requestBodies)
    {

        public IEnumerable<ILocatedOpenApiElement<IOpenApiMediaType>> GetMediaTypes() =>
            requestBodies
                .SelectMany(GetMediaTypes);

    }

    extension(ILocatedOpenApiElement<IOpenApiRequestBody> requestBody)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiMediaType>> GetMediaTypes() =>
            requestBody.Element.Content?
                .Select(p => requestBody.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region Response

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> responseSets)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiResponse>> GetResponses() =>
            responseSets
                .SelectMany(GetResponses);
    }

    extension(ILocatedOpenApiElement<OpenApiResponses> responseSet)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiResponse>> GetResponses() =>
            responseSet.Element
                .Select(p => responseSet.CreateChild(p.Value, p.Key));

        public ILocatedOpenApiElement<OpenApiUnknownResponse> GetUnknownResponse()
        {
            ArgumentNullException.ThrowIfNull(responseSet);

            return responseSet.CreateChild(_unknownResponses.GetOrCreateValue(responseSet.Element),
                OpenApiUnknownResponse.Key);
        }
    }

    extension(IEnumerable<ILocatedOpenApiElement<IOpenApiResponse>> responses)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiMediaType>> GetMediaTypes() =>
            responses
                .SelectMany(GetMediaTypes);
    }

    extension(ILocatedOpenApiElement<IOpenApiResponse> response)
    {
        public IEnumerable<ILocatedOpenApiElement<IOpenApiHeader>> GetHeaders() =>
            response.Element.Headers?
                .Select(p => response.CreateChild(p.Value, p.Key))
            ?? [];

        public IEnumerable<ILocatedOpenApiElement<IOpenApiMediaType>> GetMediaTypes() =>
            response.Element.Content?
                .Select(p => response.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region Header

    extension(ILocatedOpenApiElement<IOpenApiHeader> header)
    {
        public ILocatedOpenApiElement<IOpenApiSchema>? GetSchema() =>
            header.Element.Schema != null
                ? header.CreateChild(header.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<IOpenApiSchema> GetSchemaOrDefault() =>
            header.GetSchema() ?? header.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region MediaType

    extension(ILocatedOpenApiElement<IOpenApiMediaType> mediaType)
    {
        public ILocatedOpenApiElement<IOpenApiSchema>? GetSchema() =>
            mediaType.Element.Schema != null
                ? mediaType.CreateChild(mediaType.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<IOpenApiSchema> GetSchemaOrDefault() =>
            mediaType.GetSchema() ?? mediaType.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region Parameter

    extension(ILocatedOpenApiElement<IOpenApiParameter> parameter)
    {
        public ILocatedOpenApiElement<IOpenApiSchema>? GetSchema() =>
            parameter.Element.Schema != null
                ? parameter.CreateChild(parameter.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<IOpenApiSchema> GetSchemaOrDefault() =>
            parameter.GetSchema() ?? parameter.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region Schema

    extension(ILocatedOpenApiElement<IOpenApiSchema> schema)
    {
        public ILocatedOpenApiElement<IOpenApiSchema>? GetAdditionalProperties() =>
            schema.Element.AdditionalProperties != null
                ? schema.CreateChild(schema.Element.AdditionalProperties, "additionalProperties")
                : null;

        public ILocatedOpenApiElement<IOpenApiSchema> GetAdditionalPropertiesOrDefault() =>
            GetAdditionalProperties(schema) ?? schema.CreateChild(_defaultSchema, "additionalProperties");

        public ILocatedOpenApiElement<IOpenApiSchema>? GetItemSchema() =>
            schema.Element.Items != null
                ? schema.CreateChild(schema.Element.Items, "items")
                : null;

        public ILocatedOpenApiElement<IOpenApiSchema> GetItemSchemaOrDefault() =>
            GetItemSchema(schema) ?? schema.CreateChild(_defaultSchema, "items");

        public IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> GetProperties() =>
            schema.Element.Properties?
                .Select(p => schema.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region SecurityRequirement

    extension(ILocatedOpenApiElement<OpenApiSecurityRequirement> requirement)
    {
        public IEnumerable<KeyValuePair<ILocatedOpenApiElement<IOpenApiSecurityScheme>, IList<string>>> GetSecuritySchemes() =>
            requirement.Element
                .Select((p, index) =>
                    new KeyValuePair<ILocatedOpenApiElement<IOpenApiSecurityScheme>, IList<string>>(
                        requirement.CreateChild(p.Key, index.ToString()), p.Value));
    }

    #endregion
}
