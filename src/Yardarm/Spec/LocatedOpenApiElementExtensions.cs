using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Yardarm.Spec
{
    public static class LocatedOpenApiElementExtensions
    {
        public static LocatedOpenApiElement<T> CreateRoot<T>(this T rootItem, string key)
            where T : IOpenApiSerializable =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);

        public static IEnumerable<LocatedOpenApiElement<OpenApiPathItem>> ToLocatedElements(this OpenApiPaths paths) =>
            paths.Select(p => p.Value.CreateRoot(p.Key));

        public static IEnumerable<LocatedOpenApiElement<OpenApiOperation>> GetOperations(
            this IEnumerable<LocatedOpenApiElement<OpenApiPathItem>> paths) =>
            paths.SelectMany(
                p => p.Element.Operations,
                (path, operation) => path.CreateChild(operation.Value, operation.Key.ToString()));

        public static IEnumerable<LocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies(
            this IEnumerable<LocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetRequestBody)
                .Where(p => p != null)!;

        public static LocatedOpenApiElement<OpenApiRequestBody>? GetRequestBody(
            this LocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.RequestBody != null
                ? operation.CreateChild(operation.Element.RequestBody, "Body")
                : null;

        public static IEnumerable<LocatedOpenApiElement<OpenApiResponses>> GetResponseSets(
            this IEnumerable<LocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(GetResponseSet);

        public static LocatedOpenApiElement<OpenApiResponses> GetResponseSet(
            this LocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.CreateChild(operation.Element.Responses, "");

        public static IEnumerable<LocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this IEnumerable<LocatedOpenApiElement<OpenApiResponses>> responseSets) =>
            responseSets
                .SelectMany(p => p.GetResponses());

        public static IEnumerable<LocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this LocatedOpenApiElement<OpenApiResponses> responseSet) =>
            responseSet.Element
                .Select(p => responseSet.CreateChild(p.Value, p.Key));

        public static IEnumerable<LocatedOpenApiElement<OpenApiTag>> GetTags(
            this IEnumerable<LocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .SelectMany(p => p.Element.Tags,
                    (operation, tag) => operation.CreateChild(tag, ""));
    }
}
