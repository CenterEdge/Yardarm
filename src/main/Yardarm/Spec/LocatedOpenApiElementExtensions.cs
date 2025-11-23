using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
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
        public bool IsReference => element.Element.Reference != null;
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
            rootItems.Select(p => p.Value.CreateRoot(p.Key));
    }

    extension(OpenApiDocument document)
    {
        public ILocatedOpenApiElement<T> ResolveComponentReference<T>(OpenApiReference reference)
            where T : IOpenApiElement =>
            ((T)document.ResolveReference(reference)).CreateRoot(reference.Id);

        // These methods collect all schemas directly owned by a given object (not a reference), including recursive
        // lookups within schemas.

        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas() =>
            document.Components.Schemas.CreateRoot().SelectMany(p => p.GetAllSchemas())
                .Concat(document.Paths.CreateRoot().GetAllSchemas())
                .Concat(document.Components.RequestBodies.CreateRoot().GetAllSchemas())
                .Concat(document.Components.Responses.CreateRoot().GetAllSchemas());

        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            document.Components.Schemas.CreateRoot().SelectMany(p => p.GetAllSchemas())
                .Concat(document.Paths.CreateRoot().GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider))
                .Concat(document.Components.RequestBodies.CreateRoot().GetAllSchemas())
                .Concat(document.Components.Responses.CreateRoot().GetAllSchemas());
    }



    public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
        operations.SelectMany(p => p.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
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

    public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> requestBody) =>
        requestBody.GetMediaTypes()
            .Select(p => p.GetSchema())
            .Where(p => p is not null && !p.IsReference)!
            .SelectMany(p => p!.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
        this IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> requestBody) =>
        requestBody.GetMediaTypes()
            .Select(p => p.GetSchema())
            .Where(p => p is not null && !p.IsReference)!
            .SelectMany(p => p!.GetAllSchemas());

    public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
        this ILocatedOpenApiElement<OpenApiSchema> schema)
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
        public IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> ToLocatedElements() =>
            paths.Select(p => p.Value.CreateRoot(p.Key));
    }

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> pathItems)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas() =>
            pathItems.SelectMany(GetAllSchemas);

        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            pathItems.SelectMany(p => p.GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider));

        public IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            pathItems.SelectMany(GetOperations);

        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters() =>
            pathItems.SelectMany(GetParameters);
    }

    extension(ILocatedOpenApiElement<OpenApiPathItem> pathItem)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas() =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().GetAllSchemas());

        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            IOperationNameProvider operationNameProvider) =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().WhereOperationHasName(operationNameProvider).GetAllSchemas());

        public IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            pathItem.Element.Operations
                .Select(operation => pathItem.CreateChild(operation.Value, operation.Key.ToString()));

        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters() =>
            pathItem.Element.Parameters?
                .Select(p => pathItem.CreateChild(p, p.Name))
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

        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters() =>
            operations.SelectMany(GetParameters);

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetAllParameters() =>
            operations.SelectMany(GetAllParameters);

        public IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies() =>
            operations
                .Select(GetRequestBody)
                .Where(p => p != null)!;

        public IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets() =>
            operations
                .Select(GetResponseSet);

        public IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags() =>
            operations
                .SelectMany(GetTags);
    }

    extension(ILocatedOpenApiElement<OpenApiOperation> operation)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters() =>
            operation.Element.Parameters
                .Select(p => operation.CreateChild(p, p.Name));

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetAllParameters()
        {
            var parameters = operation.GetParameters();

            if (operation.Parent is ILocatedOpenApiElement<OpenApiPathItem> { Element.Parameters.Count: > 0 } pathItem)
            {
                // Note that DistinctBy returns the first encountered match, so the fact that operation
                // parameters are first means they will be returned in favor of path parameters
                parameters = parameters
                    .Concat(pathItem.GetParameters())
                    .DistinctBy(p => p.Key, StringComparer.Ordinal);
            }

            return parameters;
        }

        public ILocatedOpenApiElement<OpenApiRequestBody>? GetRequestBody() =>
            operation.Element.RequestBody != null
                ? operation.CreateChild(operation.Element.RequestBody, "requestBody")
                : null;

        public ILocatedOpenApiElement<OpenApiResponses> GetResponseSet() =>
            operation.CreateChild(operation.Element.Responses, "responses");

        public IEnumerable<ILocatedOpenApiElement<OpenApiSecurityRequirement>> GetSecurityRequirements() =>
            operation.Element.Security
                .Select((requirement, index) => operation.CreateChild(requirement, index.ToString()));

        public IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags() =>
            operation.Element.Tags
                .Select((tag, index) => operation.CreateChild(tag, index.ToString()));
    }

    #endregion

    #region Request

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> requestBodies)
    {

        public IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes() =>
            requestBodies
                .SelectMany(GetMediaTypes);

    }

    extension(ILocatedOpenApiElement<OpenApiRequestBody> requestBody)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes() =>
            requestBody.Element.Content?
                .Select(p => requestBody.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region Response

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> responseSets)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses() =>
            responseSets
                .SelectMany(GetResponses);
    }

    extension(ILocatedOpenApiElement<OpenApiResponses> responseSet)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses() =>
            responseSet.Element
                .Select(p => responseSet.CreateChild(p.Value, p.Key));

        public ILocatedOpenApiElement<OpenApiUnknownResponse> GetUnknownResponse()
        {
            ArgumentNullException.ThrowIfNull(responseSet);

            return responseSet.CreateChild(_unknownResponses.GetOrCreateValue(responseSet.Element),
                OpenApiUnknownResponse.Key);
        }
    }

    extension(IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> responses)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes() =>
            responses
                .SelectMany(GetMediaTypes);
    }

    extension(ILocatedOpenApiElement<OpenApiResponse> response)
    {
        public IEnumerable<ILocatedOpenApiElement<OpenApiHeader>> GetHeaders() =>
            response.Element.Headers
                .Select(p => response.CreateChild(p.Value, p.Key));

        public IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes() =>
            response.Element.Content?
                .Select(p => response.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region Header

    extension(ILocatedOpenApiElement<OpenApiHeader> header)
    {
        public ILocatedOpenApiElement<OpenApiSchema>? GetSchema() =>
            header.Element.Schema != null
                ? header.CreateChild(header.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault() =>
            header.GetSchema() ?? header.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region MediaType

    extension(ILocatedOpenApiElement<OpenApiMediaType> mediaType)
    {
        public ILocatedOpenApiElement<OpenApiSchema>? GetSchema() =>
            mediaType.Element.Schema != null
                ? mediaType.CreateChild(mediaType.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault() =>
            mediaType.GetSchema() ?? mediaType.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region Parameter

    extension(ILocatedOpenApiElement<OpenApiParameter> parameter)
    {
        public ILocatedOpenApiElement<OpenApiSchema>? GetSchema() =>
            parameter.Element.Schema != null
                ? parameter.CreateChild(parameter.Element.Schema, "schema")
                : null;

        public ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault() =>
            parameter.GetSchema() ?? parameter.CreateChild(_defaultSchema, "schema");
    }

    #endregion

    #region Schema

    extension(ILocatedOpenApiElement<OpenApiSchema> schema)
    {
        public ILocatedOpenApiElement<OpenApiSchema>? GetAdditionalProperties() =>
            schema.Element.AdditionalProperties != null
                ? schema.CreateChild(schema.Element.AdditionalProperties, "additionalProperties")
                : null;

        public ILocatedOpenApiElement<OpenApiSchema> GetAdditionalPropertiesOrDefault() =>
            GetAdditionalProperties(schema) ?? schema.CreateChild(_defaultSchema, "additionalProperties");

        public ILocatedOpenApiElement<OpenApiSchema>? GetItemSchema() =>
            schema.Element.Items != null
                ? schema.CreateChild(schema.Element.Items, "items")
                : null;

        public ILocatedOpenApiElement<OpenApiSchema> GetItemSchemaOrDefault() =>
            GetItemSchema(schema) ?? schema.CreateChild(_defaultSchema, "items");

        public IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetProperties() =>
            schema.Element.Properties?
                .Select(p => schema.CreateChild(p.Value, p.Key))
            ?? [];
    }

    #endregion

    #region SecurityRequirement

    extension(ILocatedOpenApiElement<OpenApiSecurityRequirement> requirement)
    {
        public IEnumerable<KeyValuePair<ILocatedOpenApiElement<OpenApiSecurityScheme>, IList<string>>> GetSecuritySchemes() =>
            requirement.Element
                .Select((p, index) =>
                    new KeyValuePair<ILocatedOpenApiElement<OpenApiSecurityScheme>, IList<string>>(
                        requirement.CreateChild(p.Key, index.ToString()), p.Value));
    }

    #endregion
}
