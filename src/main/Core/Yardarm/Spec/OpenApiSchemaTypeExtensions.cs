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
            schema.Type.HasValue && (schema.Type.Value & JsonSchemaType.Null) != 0;
    }
}
