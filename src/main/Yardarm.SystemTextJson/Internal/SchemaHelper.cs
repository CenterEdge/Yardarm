using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal static class SchemaHelper
    {
        public static bool IsJsonSchema(this IOpenApiElementRegistry elementRegistry,
            ClassDeclarationSyntax classDeclaration)
        {
            var element = classDeclaration.GetElementAnnotation<OpenApiSchema>(elementRegistry);
            if (element is null)
            {
                return false;
            }

            return element.IsJsonSchema();
        }

        public static bool IsJsonSchema(this ILocatedOpenApiElement<OpenApiSchema> element)
        {
            // Find the top-most schema
            while (element.Parent is ILocatedOpenApiElement<OpenApiSchema> schemaParent)
            {
                element = schemaParent;
            }

            if (element.Parent is null)
            {
                // Assume that shared component schemas may be JSON
                return true;
            }

            if (element.Parent is ILocatedOpenApiElement<OpenApiMediaType> mediaTypeElement)
            {
                return IsJsonMediaType(mediaTypeElement.Key);
            }

            // Other cases like headers aren't JSON serialized
            return false;
        }

        private static bool IsJsonMediaType(string mediaType) =>
            mediaType.EndsWith("/json") || mediaType.EndsWith("+json");
    }
}
