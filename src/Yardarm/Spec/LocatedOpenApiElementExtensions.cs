using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Spec
{
    public static class LocatedOpenApiElementExtensions
    {
        public static ILocatedOpenApiElement<T> CreateRoot<T>(this T rootItem, string key)
            where T : IOpenApiElement =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);

        public static ILocatedOpenApiElement<T> CreateChild<T>(this ILocatedOpenApiElement element,
            T child, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(child, key, element.Parents.Push(element));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> ToLocatedElements(this OpenApiPaths paths) =>
            paths.Select(p => p.Value.CreateRoot(p.Key));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations(
            this IEnumerable<ILocatedOpenApiElement<OpenApiPathItem>> paths) =>
            paths.SelectMany(
                p => p.Element.Operations,
                (path, operation) => path.CreateChild(operation.Value, operation.Key.ToString()));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetRequestBody)
                .Where(p => p != null)!;

        public static ILocatedOpenApiElement<OpenApiRequestBody>? GetRequestBody(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.RequestBody != null
                ? operation.CreateChild(operation.Element.RequestBody, "Body")
                : null;

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> GetResponseSets(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetResponseSet);

        public static ILocatedOpenApiElement<OpenApiResponses> GetResponseSet(
            this ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.CreateChild(operation.Element.Responses, "");

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this IEnumerable<ILocatedOpenApiElement<OpenApiResponses>> responseSets) =>
            responseSets
                .SelectMany(p => p.GetResponses());

        public static IEnumerable<ILocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this ILocatedOpenApiElement<OpenApiResponses> responseSet) =>
            responseSet.Element
                .Select(p => responseSet.CreateChild(p.Value, p.Key));

        public static IEnumerable<ILocatedOpenApiElement<OpenApiTag>> GetTags(
            this IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .SelectMany(p => p.Element.Tags,
                    (operation, tag) => operation.CreateChild(tag, ""));
    }
}
