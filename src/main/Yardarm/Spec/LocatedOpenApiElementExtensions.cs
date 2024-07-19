using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;

namespace Yardarm.Spec
{
    public static class LocatedOpenApiElementExtensions
    {
        private static readonly ConditionalWeakTable<OpenApiResponses, OpenApiUnknownResponse> _unknownResponses =
            new ConditionalWeakTable<OpenApiResponses, OpenApiUnknownResponse>();

        private static readonly OpenApiSchema _defaultSchema = new OpenApiSchema();

        public static bool IsRoot(this ILocatedOpenApiElement element) => element.Parent is null;

        public static bool IsReference<T>(this ILocatedOpenApiElement<T> element)
            where T : IOpenApiReferenceable =>
            element.Element.Reference != null;

        public static ILocatedOpenApiElement<T> CreateRoot<T>(this T rootItem, string key)
            where T : IOpenApiElement =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);

        public static IEnumerable<ILocatedOpenApiElement<T>> CreateRoot<T>(
            this IEnumerable<KeyValuePair<string, T>> rootItems)
            where T : IOpenApiElement =>
            rootItems.Select(p => p.Value.CreateRoot(p.Key));

        public static ILocatedOpenApiElement<T> CreateChild<T>(this ILocatedOpenApiElement element,
            T child, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(child, key, element);

        public static IEnumerable<ILocatedOpenApiElement> Parents(this ILocatedOpenApiElement element)
        {
            var current = element;
            while (current.Parent != null)
            {
                current = current.Parent;
                yield return current;
            }
        }

        public static ILocatedOpenApiElement<T> ResolveComponentReference<T>(this OpenApiDocument document,
            OpenApiReference reference)
            where T : IOpenApiElement =>
            ((T)document.ResolveReference(reference)).CreateRoot(reference.Id);

        #region GetAllSchemas

        // These methods collect all schemas directly owned by a given object (not a reference), including recursive
        // lookups within schemas.

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(this OpenApiDocument document) =>
            document.Components.Schemas.CreateRoot().SelectMany(p => p.GetAllSchemas())
                .Concat(document.Paths.CreateRoot().GetAllSchemas())
                .Concat(document.Components.RequestBodies.CreateRoot().GetAllSchemas())
                .Concat(document.Components.Responses.CreateRoot().GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            this OpenApiDocument document,
            IOperationNameProvider operationNameProvider) =>
            document.Components.Schemas.CreateRoot().SelectMany(p => p.GetAllSchemas())
                .Concat(document.Paths.CreateRoot().GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider))
                .Concat(document.Components.RequestBodies.CreateRoot().GetAllSchemas())
                .Concat(document.Components.Responses.CreateRoot().GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> pathItems) =>
            pathItems.SelectMany(GetAllSchemas);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this ILocatedOpenApiElement<OpenApiPathItem> pathItem) =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            this IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> pathItems,
            IOperationNameProvider operationNameProvider) =>
            pathItems.SelectMany(p => p.GetAllSchemasExcludingOperationsWithoutNames(operationNameProvider));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemasExcludingOperationsWithoutNames(
            this ILocatedOpenApiElement<OpenApiPathItem> pathItem,
            IOperationNameProvider operationNameProvider) =>
            pathItem.GetParameters().SelectMany(p => p.GetSchemaOrDefault().GetAllSchemas())
                .Concat(pathItem.GetOperations().WhereOperationHasName(operationNameProvider).GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations.SelectMany(p => p.GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            var requestBody = operation.GetRequestBody();
            if (requestBody is not null && !requestBody.IsReference())
            {
                var requestSchemas = requestBody
                    .GetMediaTypes()
                    .Select(p => p.GetSchema())
                    .Where(p => p is not null && !p.IsReference())
                    .SelectMany(p => p!.GetAllSchemas());

                foreach (var schema in requestSchemas)
                {
                    yield return schema;
                }
            }

            foreach (var responseSchema in operation
                         .GetResponseSet()
                         .GetResponses()
                         .Where(p => !p.IsReference())
                         .GetAllSchemas())
            {
                yield return responseSchema;
            }
        }

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> requestBody) =>
            requestBody.GetMediaTypes()
                .Select(p => p.GetSchema())
                .Where(p => p is not null && !p.IsReference())!
                .SelectMany(p => p!.GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> requestBody) =>
            requestBody.GetMediaTypes()
                .Select(p => p.GetSchema())
                .Where(p => p is not null && !p.IsReference())!
                .SelectMany(p => p!.GetAllSchemas());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetAllSchemas(
            this ILocatedOpenApiElement<OpenApiSchema> schema)
        {
            yield return schema;

            var itemSchema = schema.GetItemSchema();
            if (itemSchema is not null && !itemSchema.IsReference())
            {
                foreach (var childSchema in itemSchema.GetAllSchemas())
                {
                    yield return childSchema;
                }
            }

            foreach (var childSchema in schema.GetProperties()
                         .Where(p => !p.IsReference())
                         .SelectMany(p => p.GetAllSchemas()))
            {
                yield return childSchema;
            }
        }

        #endregion

        #region PathItem

        public static IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> ToLocatedElements(this OpenApiPaths paths) =>
            paths.Select(p => p.Value.CreateRoot(p.Key));

        #endregion

        #region Operation

        public static IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations(
            this IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> paths) =>
            paths.SelectMany(GetOperations);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations(
            this ILocatedOpenApiElement<OpenApiPathItem> path) =>
            path.Element.Operations
                .Select(operation => path.CreateChild(operation.Value, operation.Key.ToString()));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> WhereOperationHasName(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations, IOperationNameProvider operationNameProvider) =>
            operations
                .Where(operation => !string.IsNullOrEmpty(operationNameProvider.GetOperationName(operation)));

        #endregion

        #region Parameters

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations.SelectMany(GetParameters);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.Parameters
                .Select(p => operation.CreateChild(p, p.Name));

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetAllParameters(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations.SelectMany(GetAllParameters);

        /// <summary>
        /// Gets all operation parameters including parameters defined on the path, if applicable.
        /// Duplicates are treated as overrides and the operation parameter is returned.
        /// </summary>
        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetAllParameters(
            this ILocatedOpenApiElement<OpenApiOperation> operation)
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

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> pathItems) =>
            pathItems.SelectMany(GetParameters);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this ILocatedOpenApiElement<OpenApiPathItem> pathItem) =>
            pathItem.Element.Parameters?
                .Select(p => pathItem.CreateChild(p, p.Name))
            ?? Enumerable.Empty<ILocatedOpenApiElement<OpenApiParameter>>();

        #endregion

        #region RequestBody

        public static IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetRequestBody)
                .Where(p => p != null)!;

        public static ILocatedOpenApiElement<OpenApiRequestBody>? GetRequestBody(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.RequestBody != null
                ? operation.CreateChild(operation.Element.RequestBody, "requestBody")
                : null;

        #endregion

        #region ResponseSet

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetResponseSet);

        public static ILocatedOpenApiElement<OpenApiResponses> GetResponseSet(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.CreateChild(operation.Element.Responses, "responses");

        #endregion

        #region Response

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> responseSets) =>
            responseSets
                .SelectMany(GetResponses);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this ILocatedOpenApiElement<OpenApiResponses> responseSet) =>
            responseSet.Element
                .Select(p => responseSet.CreateChild(p.Value, p.Key));

        public static ILocatedOpenApiElement<OpenApiUnknownResponse> GetUnknownResponse(
            this ILocatedOpenApiElement<OpenApiResponses> responses)
        {
            ArgumentNullException.ThrowIfNull(responses);

            return responses.CreateChild(_unknownResponses.GetOrCreateValue(responses.Element),
                OpenApiUnknownResponse.Key);
        }

        #endregion

        #region Tag

        public static IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .SelectMany(GetTags);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.Tags
                .Select((tag, index) => operation.CreateChild(tag, index.ToString()));

        #endregion

        #region Header

        public static IEnumerable<ILocatedOpenApiElement<OpenApiHeader>> GetHeaders(
            this ILocatedOpenApiElement<OpenApiResponse> response) =>
            response.Element.Headers
                .Select(p => response.CreateChild(p.Value, p.Key));

        #endregion

        #region MediaType

        public static IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes(
            this IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> requestBody) =>
            requestBody
                .SelectMany(GetMediaTypes);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes(
            this ILocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            requestBody.Element.Content?
                .Select(p => requestBody.CreateChild(p.Value, p.Key))
            ?? Enumerable.Empty<ILocatedOpenApiElement<OpenApiMediaType>>();

        public static IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes(
            this IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> response) =>
            response
                .SelectMany(GetMediaTypes);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes(
            this ILocatedOpenApiElement<OpenApiResponse> response) =>
            response.Element.Content?
                .Select(p => response.CreateChild(p.Value, p.Key))
            ?? Enumerable.Empty<ILocatedOpenApiElement<OpenApiMediaType>>();

        #endregion

        #region Schema

        public static ILocatedOpenApiElement<OpenApiSchema>? GetAdditionalProperties(
            this ILocatedOpenApiElement<OpenApiSchema> schema) =>
            schema.Element.AdditionalProperties != null
                ? schema.CreateChild(schema.Element.AdditionalProperties, "additionalProperties")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetAdditionalPropertiesOrDefault(
            this ILocatedOpenApiElement<OpenApiSchema> schema) =>
            GetAdditionalProperties(schema) ?? schema.CreateChild(_defaultSchema, "additionalProperties");

        public static ILocatedOpenApiElement<OpenApiSchema>? GetItemSchema(
            this ILocatedOpenApiElement<OpenApiSchema> schema) =>
            schema.Element.Items != null
                ? schema.CreateChild(schema.Element.Items, "items")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetItemSchemaOrDefault(
            this ILocatedOpenApiElement<OpenApiSchema> schema) =>
            GetItemSchema(schema) ?? schema.CreateChild(_defaultSchema, "items");

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> GetProperties(
            this ILocatedOpenApiElement<OpenApiSchema> schema) =>
            schema.Element.Properties?
                .Select(p => schema.CreateChild(p.Value, p.Key))
            ?? Enumerable.Empty<ILocatedOpenApiElement<OpenApiSchema>>();

        public static ILocatedOpenApiElement<OpenApiSchema>? GetSchema(
            this ILocatedOpenApiElement<OpenApiHeader> header) =>
            header.Element.Schema != null
                ? header.CreateChild(header.Element.Schema, "schema")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault(
            this ILocatedOpenApiElement<OpenApiHeader> header) =>
            header.GetSchema() ?? header.CreateChild(_defaultSchema, "schema");

        public static ILocatedOpenApiElement<OpenApiSchema>? GetSchema(
            this ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            mediaType.Element.Schema != null
                ? mediaType.CreateChild(mediaType.Element.Schema, "schema")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault(
            this ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            mediaType.GetSchema() ?? mediaType.CreateChild(_defaultSchema, "schema");

        public static ILocatedOpenApiElement<OpenApiSchema>? GetSchema(
            this ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            parameter.Element.Schema != null
                ? parameter.CreateChild(parameter.Element.Schema, "schema")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault(
            this ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            parameter.GetSchema() ?? parameter.CreateChild(_defaultSchema, "schema");

        #endregion

        #region SecurityRequirement

        public static IEnumerable<ILocatedOpenApiElement<OpenApiSecurityRequirement>> GetSecurityRequirements(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.Security
                .Select((requirement, index) => operation.CreateChild(requirement, index.ToString()));

        #endregion

        #region SecurityRequirement

        public static IEnumerable<KeyValuePair<ILocatedOpenApiElement<OpenApiSecurityScheme>, IList<string>>>
            GetSecuritySchemes(this ILocatedOpenApiElement<OpenApiSecurityRequirement> requirement) =>
            requirement.Element
                .Select((p, index) =>
                    new KeyValuePair<ILocatedOpenApiElement<OpenApiSecurityScheme>, IList<string>>(
                        requirement.CreateChild(p.Key, index.ToString()), p.Value));

        #endregion
    }
}
