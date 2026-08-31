using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for externally discriminated unions that implement <see cref="IUnion"/>.
/// Constructors must be annotated with <see cref="UnionCaseNameAttribute"/>.
/// </summary>
/// <typeparam name="T">Type of the union.</typeparam>
internal sealed class JsonExternallyDiscriminatedUnionConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    : JsonConverter<T>
    where T : IUnion
{
    public override bool HandleNull => false;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        // Advance to the first property or EndObject token
        if (!reader.Read())
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        T? result = default;
        bool foundResult = false;
        string? firstPropertyName = null;
        JsonElement? firstPropertyValue = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }

            string? propertyName = reader.GetString();
            firstPropertyName ??= propertyName;

            if (!foundResult
                && propertyName is not null
                && ExternallyDiscriminatedUnion<T>.CasesByName.TryGetValue(propertyName, out var caseInfo))
            {
                // Advance to the value token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }

                JsonTypeInfo typeInfo = options.GetTypeInfo(caseInfo.CaseType);
                object? value = JsonSerializer.Deserialize(ref reader, typeInfo);
                if (value is null)
                {
                    // Null is not a valid union case
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }

                result = caseInfo.Factory(value);
                foundResult = true;

                // Advance past the value token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }
            }
            else
            {
                // Advance to the value token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }

                if (firstPropertyValue is null)
                {
                    // Parse and save the first matching property's content in case we need it for the unknown case
                    firstPropertyValue = JsonElement.ParseValue(ref reader);
                }
                else
                {
                    // Skip the property if we have already found a matching case
                    reader.Skip();
                }

                // Read to the next property or EndObject token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }
            }
        }

        return foundResult ? result : CreateUnknownCase(firstPropertyName, firstPropertyValue);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        object? innerValue = value.Value;
        if (innerValue is not null)
        {
            writer.WriteStartObject();

            if (innerValue is UnknownCase unknownCase)
            {
                // Support round-trip serialization of the unknown case

                if (unknownCase.CaseName is not null)
                {
                    writer.WritePropertyName(unknownCase.CaseName);

                    if (unknownCase.Value is JsonElement unknownValue)
                    {
                        unknownValue.WriteTo(writer);
                    }
                    else
                    {
                        writer.WriteNullValue();
                    }
                }

                writer.WriteEndObject();
                return;
            }

            Type? innerType = innerValue.GetType();
            do
            {
                if (ExternallyDiscriminatedUnion<T>.CasesByType.TryGetValue(innerType, out var caseInfo))
                {
                    writer.WritePropertyName(caseInfo.CaseName);

                    JsonTypeInfo typeInfo = options.GetTypeInfo(caseInfo.CaseType);
                    JsonSerializer.Serialize(writer, innerValue, typeInfo);

                    writer.WriteEndObject();
                    return;
                }

                // Try the parent type to handle serializing inherited types, with the understanding that
                // it may not be deserializable without discriminators.
                innerType = innerType.BaseType;
            } while (innerType is not null);
        }

        // The union is invalid and contains a type for which it doesn't have a constructor.
        JsonDiscriminatedUnionConverter.ThrowUnknownUnionCaseType(typeof(T), innerValue?.GetType());
    }

    private static T CreateUnknownCase(string? caseName = null, object? value = null)
    {
        if (ExternallyDiscriminatedUnion<T>.UnknownCaseFactory is null)
        {
            JsonDiscriminatedUnionConverter.ThrowUnknownUnionCase(typeof(T), caseName);
        }

        return ExternallyDiscriminatedUnion<T>.UnknownCaseFactory(new UnknownCase(caseName, value));
    }
}

internal static class JsonDiscriminatedUnionConverter
{
    [DoesNotReturn]
    public static void ThrowInvalidUnionJson(Type unionType)
        => throw new JsonException($"Invalid JSON for union type '{unionType.FullName}'.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCaseType(Type unionType, Type? caseType)
        => throw new JsonException(caseType is not null
            ? $"Union type '{unionType.FullName}' does not have a case with type '{caseType.FullName}'."
            : $"Union type '{unionType.FullName}' may not contain a null value.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCase(Type unionType, string? caseName)
        => throw new JsonException(caseName is not null
            ? $"Union type '{unionType.FullName}' does not have a case '{caseName}'."
            : $"Union type '{unionType.FullName}' does not have an unknown case.");
}
