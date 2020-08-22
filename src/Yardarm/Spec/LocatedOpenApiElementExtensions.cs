using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Spec
{
    public static class LocatedOpenApiElementExtensions
    {
        private static readonly ConditionalWeakTable<OpenApiResponses, OpenApiUnknownResponse> _unknownResponses =
            new ConditionalWeakTable<OpenApiResponses, OpenApiUnknownResponse>();

        private static readonly OpenApiSchema _defaultSchema =
            new OpenApiSchema
            {
                Type = "string"
            };

        public static ILocatedOpenApiElement<T> CreateRoot<T>(this T rootItem, string key)
            where T : IOpenApiElement =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);

        public static ILocatedOpenApiElement<T> CreateChild<T>(this ILocatedOpenApiElement element,
            T child, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(child, key, element.Parents.Push(element));

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

        #endregion

        #region Parameters

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations.SelectMany(GetParameters);

        public static IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> GetParameters(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.Parameters
                .Select(p => operation.CreateChild(p, p.Name));

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
            if (responses == null)
            {
                throw new ArgumentNullException(nameof(responses));
            }

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

        public static ILocatedOpenApiElement<OpenApiSchema>? GetSchema(
            this ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            parameter.Element.Schema != null
                ? parameter.CreateChild(parameter.Element.Schema, "schema")
                : null;

        public static ILocatedOpenApiElement<OpenApiSchema> GetSchemaOrDefault(
            this ILocatedOpenApiElement<OpenApiParameter> parameter) =>
            parameter.GetSchema() ?? parameter.CreateChild(_defaultSchema, "schema");

        #endregion
    }
}
