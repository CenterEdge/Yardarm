namespace Yardarm.SystemTextJson;

public sealed class JsonOptions
{
    // Starting with Yardarm 0.8, we enforce required properties, constructor parameters, and non-nullable reference types
    // by default. This can be disabled by passing the properties as false. This provides a more consistent experience
    // around nullable reference types and required properties, but may break existing code that relies on the previous behavior.
    // In particular, servers that misbehave and return null or missing properties when their spec says the property will be present
    // will throw deserialization exceptions.

    public bool EnforceRequiredProperties { get; set; } = true;
    public bool RespectNullableAnnotations { get; set; } = true;
    public bool RespectRequiredConstructorParameters { get; set; } = true;

    public void ApplySettings(YardarmGenerationSettings settings)
    {
        if (settings.Properties.TryGetValue("JsonEnforceRequiredProperties", out string? enforceRequiredProperties)
                && bool.TryParse(enforceRequiredProperties, out bool enforceRequiredPropertiesBool))
        {
            EnforceRequiredProperties = enforceRequiredPropertiesBool;
        }

        if (settings.Properties.TryGetValue("JsonRespectNullableAnnotations", out string? respectNullableAnnotations)
            && bool.TryParse(respectNullableAnnotations, out bool respectNullableAnnotationsBool))
        {
            RespectNullableAnnotations = respectNullableAnnotationsBool;
        }

        if (settings.Properties.TryGetValue("JsonRespectRequiredConstructorParameters", out string? respectRequiredConstructorParameters)
            && bool.TryParse(respectRequiredConstructorParameters, out bool respectRequiredConstructorParametersBool))
        {
            RespectRequiredConstructorParameters = respectRequiredConstructorParametersBool;
        }
    }
}
