using System.Collections.Generic;
using System.Linq;
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

        public static bool IsPolymorphic(OpenApiSchema schema) =>
            schema is {Discriminator.PropertyName: not null} or {OneOf.Count: > 0};


        private static bool IsJsonMediaType(string mediaType) =>
            mediaType.EndsWith("/json") || mediaType.EndsWith("+json");

        /// <summary>
        /// Collects a list of all discriminator keys and their relevant C# type.
        /// </summary>
        public static IEnumerable<(string key, TypeSyntax typeName)> GetDiscriminatorMappings(GenerationContext context,
            ILocatedOpenApiElement<OpenApiSchema> element) =>
            GetMappings(context, element)
                .Select(p => (p.Key, context.TypeGeneratorRegistry.Get(p.Schema).TypeInfo.Name))
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                .Where(p => p.Item2 != null);

        /// <summary>
        /// Collects the list of value to schema mappings defined for the type, choosing from the
        /// best source for various kinds of mappings and polymorphism.
        /// </summary>
        /// <remarks>
        /// The preferred choice is specifically defined mappings on the discriminator. However, when
        /// missing it will fallback all oneOf's defined on the type. If that is not the case, it will
        /// look for cases of allOf inheritance from schemas defined in the components section.
        /// </remarks>
        private static IEnumerable<(string Key, ILocatedOpenApiElement<OpenApiSchema> Schema)> GetMappings(
            GenerationContext context,
            ILocatedOpenApiElement<OpenApiSchema> element)
        {
            if (element.Element.Discriminator is {Mapping.Count: > 0})
            {
                // Use specifically listed mappings
                return element.Element.Discriminator.Mapping
                    .Select(p =>
                    {
                        // TODO: We should really be parsing this rather than just checking a prefix, but the OpenAPI parser isn't exposed
                        if (p.Value.StartsWith("#/components/schemas/"))
                        {
                            string schemaName = p.Value.Substring("#/components/schemas/".Length);
                            if (context.Document.Components.Schemas.TryGetValue(schemaName, out var schema))
                            {
                                return (p.Key, Schema: schema.CreateRoot(p.Key));
                            }
                        }

                        return (p.Key, Schema: null!);
                    })
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    .Where(p => p.Schema is not null);
            }

            if (element.Element.OneOf is {Count: > 0})
            {
                // Gather mappings from "oneOf" that get a default mapping based on the schema name
                return element.Element.OneOf
                    .Where(p => p.Reference is not null)
                    .Select(p => (p.Reference.Id, p.CreateRoot(p.Reference.Id)));
            }

            // Find other schemas that reference this one using allOf. This only applies to base
            // classes, don't try this with interfaces.
            return context.Document.Components.Schemas
                .Where(p =>
                {
                    var firstAllOf = p.Value.AllOf?.FirstOrDefault();
                    return firstAllOf?.Reference is not null &&
                           firstAllOf.Reference.Id == element.Key;
                })
                .Select(p => (p.Key, p.Value.CreateRoot(p.Key)));
        }
    }
}
