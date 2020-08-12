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
                .Where(p => p.Element.RequestBody != null)
                .Select(p => p.CreateChild(p.Element.RequestBody, "Body"));

        public static IEnumerable<LocatedOpenApiElement<OpenApiResponses>> GetResponseSets(
            this IEnumerable<LocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .Select(p => p.CreateChild(p.Element.Responses, ""));

        public static IEnumerable<LocatedOpenApiElement<OpenApiResponse>> GetResponses(
            this IEnumerable<LocatedOpenApiElement<OpenApiResponses>> operations) =>
            operations
                .SelectMany(p => p.Element,
                    (operation, response) => operation.CreateChild(response.Value, response.Key));

        public static IEnumerable<LocatedOpenApiElement<OpenApiTag>> GetTags(
            this IEnumerable<LocatedOpenApiElement<OpenApiOperation>> operations) =>
            operations
                .SelectMany(p => p.Element.Tags,
                    (operation, tag) => operation.CreateChild(tag, ""));
    }
}
