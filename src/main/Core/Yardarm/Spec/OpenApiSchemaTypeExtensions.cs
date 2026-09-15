using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;

namespace Yardarm.Spec;

/// <summary>
/// Extension methods for working with the JsonSchemaType flagged enum on IOpenApiSchema.
/// </summary>
public static class OpenApiSchemaTypeExtensions
{
    extension(IOpenApiSchema schema)
    {
        /// <summary>
        /// Checks if the schema's Type equals the specified type after removing
        /// the Null flag (which represents nullable in OpenAPI 3.1).
        /// </summary>
        public bool IsType(JsonSchemaType type) =>
            schema.Type.HasValue && (schema.Type.Value & ~JsonSchemaType.Null) == type;

        /// <summary>
        /// Gets whether the schema's Type includes the Null flag.
        /// </summary>
        public bool Nullable =>
            (schema.Type.HasValue && (schema.Type.Value & JsonSchemaType.Null) != 0)
            || schema.TryGetNullableUnderlyingSchema(out _);

        /// <summary>
        /// Gets the non-null alternative from a nullable <c>oneOf</c> schema.
        /// </summary>
        /// <remarks>
        /// A nullable <c>oneOf</c> schema has no type, composition, or properties of its own and has
        /// exactly two alternatives: one null-only schema and one non-null schema.
        /// </remarks>
        public bool TryGetNullableUnderlyingSchema([NotNullWhen(true)] out IOpenApiSchema? underlyingSchema)
        {
            if (schema.Type is not null
                || schema.AllOf is { Count: > 0 }
                || schema.AnyOf is { Count: > 0 }
                || schema.Properties is { Count: > 0 }
                || schema.OneOf is not { Count: 2 } oneOf)
            {
                underlyingSchema = null;
                return false;
            }

            underlyingSchema = oneOf[0].Type == JsonSchemaType.Null
                ? oneOf[1]
                : oneOf[1].Type == JsonSchemaType.Null ? oneOf[0] : null;
            return underlyingSchema?.Type is { } underlyingType
                && (underlyingType & JsonSchemaType.Null) == 0;
        }
    }
}
