using System;
using System.Text.Json.Serialization;

namespace Yardarm.SystemTextJson;

public sealed class JsonOptions
{
    /// <summary>
    /// Enable JSON strict mode, which will throw exceptions for many cases during deserialization.
    /// Rules may be relaxed by setting other properties on this class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If set to <c>true</c>:
    /// - Required properties must be present in the JSON payload
    /// - Non-nullable properties may not have a null value in the JSON payload
    /// - Property names are case-sensitive
    /// - Duplicate property names are not allowed in the JSON payload
    /// - Numbers may not be provided as strings in the JSON payload
    /// </para>
    /// </remarks>
    public bool Strict { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.AllowDuplicateProperties"/>.
    /// </summary>
    public bool? AllowDuplicateProperties { get; set; }

    /// <summary>
    /// If set to <c>true</c>, JSON deserialization will throw an exception if a required property is missing.
    /// If set to <c>false</c>, missing required properties will be ignored.
    /// If set to <c>null</c>, the default behavior is based on the <see cref="Strict"/> property.
    /// </summary>
    public bool? EnforceRequiredProperties { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.NumberHandling"/>.
    /// </summary>
    public JsonNumberHandling? NumberHandling { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.PropertyNameCaseInsensitive"/>.
    /// </summary>
    public bool? PropertyNameCaseInsensitive { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.RespectNullableAnnotations"/>.
    /// </summary>
    public bool? RespectNullableAnnotations { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.RespectRequiredConstructorParameters"/>.
    /// </summary>
    public bool? RespectRequiredConstructorParameters { get; set; }

    /// <summary>
    /// Overrides the default behavior of <see cref="System.Text.Json.JsonSerializerOptions.UnmappedMemberHandling"/>.
    /// </summary>
    public JsonUnmappedMemberHandling? UnmappedMemberHandling { get; set; }

    // Automatically enforce required properties if Strict is enabled, unless explicitly overridden by EnforceRequiredProperties.
    internal bool EffectiveEnforceRequiredProperties => EnforceRequiredProperties ?? Strict;

    // By default, even in strict-mode, allow unknown properties. Almost any server implementation may add new properties to the payload,
    // and we don't want to break clients when that happens. If a user wants to enforce strict behavior, they should explicitly set UnmappedMemberHandling to Disallow.
    internal JsonUnmappedMemberHandling EffectiveUnmappedMemberHandling => UnmappedMemberHandling ?? JsonUnmappedMemberHandling.Skip;

    internal void ApplySettings(YardarmGenerationSettings settings)
    {
        if (settings.Properties.TryGetValue("JsonStrict", out string? strict)
            && bool.TryParse(strict, out bool strictBool))
        {
            Strict = strictBool;
        }

        if (settings.Properties.TryGetValue("JsonAllowDuplicateProperties", out string? allowDuplicateProperties)
            && bool.TryParse(allowDuplicateProperties, out bool allowDuplicatePropertiesBool))
        {
            AllowDuplicateProperties = allowDuplicatePropertiesBool;
        }

        if (settings.Properties.TryGetValue("JsonEnforceRequiredProperties", out string? enforceRequiredProperties)
            && bool.TryParse(enforceRequiredProperties, out bool enforceRequiredPropertiesBool))
        {
            EnforceRequiredProperties = enforceRequiredPropertiesBool;
        }

        if (settings.Properties.TryGetValue("JsonNumberHandling", out string? numberHandling))
        {
            var numberHandlingEnum = JsonNumberHandling.Strict;

            foreach (var value in numberHandling.Split(','))
            {
                if (Enum.TryParse(value, ignoreCase: true, out JsonNumberHandling parsedValue))
                {
                    numberHandlingEnum |= parsedValue;
                }
            }

            NumberHandling = numberHandlingEnum;
        }

        if (settings.Properties.TryGetValue("JsonPropertyNameCaseInsensitive", out string? propertyNameCaseInsensitive)
            && bool.TryParse(propertyNameCaseInsensitive, out bool propertyNameCaseInsensitiveBool))
        {
            PropertyNameCaseInsensitive = propertyNameCaseInsensitiveBool;
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

        if (settings.Properties.TryGetValue("JsonUnmappedMemberHandling", out string? unmappedMemberHandling)
            && Enum.TryParse(unmappedMemberHandling, ignoreCase: true, out JsonUnmappedMemberHandling unmappedMemberHandlingEnum))
        {
            UnmappedMemberHandling = unmappedMemberHandlingEnum;
        }
    }
}
