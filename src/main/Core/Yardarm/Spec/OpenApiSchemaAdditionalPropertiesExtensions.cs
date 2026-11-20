using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Yardarm.Spec;

/// <summary>
/// Extension methods for determining whether an object schema permits undeclared properties, and which schema
/// those properties must match.
/// </summary>
/// <remarks>
/// <para>
/// JSON Schema 2020-12 allows <c>additionalProperties</c> and <c>unevaluatedProperties</c> on the same schema.
/// <c>additionalProperties</c> marks every property that <c>properties</c> and <c>patternProperties</c> do not
/// match as evaluated, so an adjacent <c>unevaluatedProperties</c> has no effect. Where both are present,
/// <c>additionalProperties</c> decides.
/// </para>
/// <para>
/// Microsoft.OpenApi reads an absent <c>additionalProperties</c> and <c>additionalProperties: true</c> into the
/// same object model. Yardarm therefore treats <c>additionalProperties: true</c> as absent, and an adjacent
/// <c>unevaluatedProperties</c> then applies.
/// </para>
/// </remarks>
public static class OpenApiSchemaAdditionalPropertiesExtensions
{
    private const string AbstractExtensionName = "x-abstract";

    extension(IOpenApiSchema schema)
    {
        /// <summary>
        /// Gets whether the schema permits properties beyond those it declares, so a generated model needs an
        /// <c>AdditionalProperties</c> dictionary.
        /// </summary>
        /// <remarks>
        /// A schema is closed by <c>additionalProperties: false</c>, by <c>unevaluatedProperties: false</c> without
        /// an <c>additionalProperties</c> subschema, or by <c>x-abstract: true</c>. An abstract schema is open in the
        /// document so that the schemas extending it with <c>allOf</c> validate, but no instance of it exists apart
        /// from those schemas. Whether undeclared properties are allowed is therefore decided by each derived schema.
        /// </remarks>
        public bool AllowsUndeclaredProperties =>
            !schema.IsAbstract
            && (schema.AdditionalProperties is not null
                || (schema.AdditionalPropertiesAllowed && schema.UnevaluatedProperties));

        /// <summary>
        /// Gets the schema that undeclared properties must match: the <c>additionalProperties</c> subschema, or else
        /// the <c>unevaluatedProperties</c> subschema. Returns <see langword="null"/> when neither is a subschema.
        /// </summary>
        public IOpenApiSchema? UndeclaredPropertiesSchema =>
            schema.AdditionalProperties
            ?? (schema as IOpenApiSchemaMissingProperties)?.UnevaluatedPropertiesSchema;

        /// <summary>
        /// Gets whether the schema has <c>x-abstract: true</c>.
        /// </summary>
        public bool IsAbstract =>
            schema.Extensions?.TryGetValue(AbstractExtensionName, out IOpenApiExtension? extension) == true
            && extension is JsonNodeExtension { Node: JsonValue value }
            && value.TryGetValue(out bool isAbstract)
            && isAbstract;
    }
}
