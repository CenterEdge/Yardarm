using Microsoft.OpenApi;

namespace Yardarm.Spec;

/// <summary>
/// Extension methods for working with the JsonSchemaType flagged enum on IOpenApiSchema.
/// </summary>
public static class OpenApiSchemaTypeExtensions
{
    /// <summary>
    /// Checks if the schema's Type includes the specified type flag,
    /// ignoring the Null flag (which represents nullable in OpenAPI 3.1).
    /// </summary>
    public static bool HasType(this IOpenApiSchema schema, JsonSchemaType type) =>
        schema.Type.HasValue && (schema.Type.Value & ~JsonSchemaType.Null & type) != 0;

    /// <summary>
    /// Checks if the schema is nullable (Type includes the Null flag).
    /// This replaces the old Schema.Nullable property.
    /// </summary>
    public static bool IsNullable(this IOpenApiSchema schema) =>
        schema.Type.HasValue && (schema.Type.Value & JsonSchemaType.Null) != 0;
}
