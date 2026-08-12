using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal;

internal static class SchemaHelper
{

    extension(IOpenApiElementRegistry elementRegistry)
    {
        public bool IsJsonSchema(ClassDeclarationSyntax classDeclaration)
        {
            var element = classDeclaration.GetElementAnnotation<OpenApiSchema>(elementRegistry);
            if (element is null)
            {
                return false;
            }

            return element.IsJsonSchema;
        }
    }

    extension(ILocatedOpenApiElement<IOpenApiSchema> element)
    {
        public bool IsJsonSchema
        {
            get
            {
                // Find the top-most schema
                while (element.Parent is ILocatedOpenApiElement<IOpenApiSchema> schemaParent)
                {
                    element = schemaParent;
                }

                if (element.Parent is null)
                {
                    // Assume that shared component schemas may be JSON
                    return true;
                }

                if (element.Parent is ILocatedOpenApiElement<IOpenApiMediaType> mediaTypeElement)
                {
                    return IsJsonMediaType(mediaTypeElement.Key);
                }

                // Other cases like headers aren't JSON serialized
                return false;
            }
        }
    }

    public static bool IsPolymorphic(IOpenApiSchema schema) =>
        schema is {Discriminator.PropertyName: not null} or {OneOf.Count: > 0};


    private static bool IsJsonMediaType(string mediaType) =>
        mediaType.EndsWith("/json") || mediaType.EndsWith("+json");

    /// <summary>
    /// Collects a list of all discriminator keys and their relevant C# type.
    /// </summary>
    public static IEnumerable<(string key, TypeSyntax typeName)> GetDiscriminatorMappings(GenerationContext context,
        ILocatedOpenApiElement<IOpenApiSchema> element) =>
        GetMappings(context, element)
            .Select(p => (p.Key, context.TypeGeneratorRegistry.Get(p.Schema).TypeInfo.Name))
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Where(p => p.Name != null);

    /// <summary>
    /// Collects the list of value to schema mappings defined for the type, choosing from the
    /// best source for various kinds of mappings and polymorphism.
    /// </summary>
    /// <remarks>
    /// The preferred choice is specifically defined mappings on the discriminator. However, when
    /// missing it will fallback all oneOf's defined on the type. If that is not the case, it will
    /// look for cases of allOf inheritance from schemas defined in the components section.
    /// </remarks>
    private static IEnumerable<(string Key, ILocatedOpenApiElement<IOpenApiSchema> Schema)> GetMappings(
        GenerationContext context,
        ILocatedOpenApiElement<IOpenApiSchema> element)
    {
        if (element.Element.Discriminator is {Mapping.Count: > 0})
        {
            // Use specifically listed mappings — Mapping values are now OpenApiSchemaReference objects
            return element.Element.Discriminator.Mapping
                .Select(p =>
                {
                    // p.Value is an OpenApiSchemaReference which implements IOpenApiSchema
                    var referenceId = p.Value.GetReferenceId();
                    if (referenceId is not null &&
                        (context.Document.Components?.Schemas?.TryGetValue(referenceId, out var schema) ?? false))
                    {
                        return (p.Key, Schema: schema.CreateRoot(p.Key));
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
                .Where(p => p is IOpenApiReferenceHolder)
                .Select(p => (p.GetReferenceId()!, p.CreateRoot(p.GetReferenceId()!)));
        }

        // Find other schemas that reference this one using allOf. This only applies to base
        // classes, don't try this with interfaces.
        return ((IEnumerable<KeyValuePair<string, IOpenApiSchema>>?)context.Document.Components?.Schemas ?? [])
            .Where(p =>
            {
                var firstAllOf = p.Value.AllOf?.FirstOrDefault();
                return firstAllOf is IOpenApiReferenceHolder &&
                       firstAllOf.GetReferenceId() == element.Key;
            })
            .Select(p => (p.Key, p.Value.CreateRoot(p.Key)));
    }
}
