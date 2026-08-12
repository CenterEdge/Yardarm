using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for externally discriminated unions that implement <see cref="IUnion"/>.
/// Constructors must be annotated with <see cref="UnionCaseNameAttribute"/>.
/// </summary>
/// <typeparam name="T">Type of the union.</typeparam>
internal sealed class JsonExternallyDiscriminatedUnionConverter<T> : JsonConverter
    where T : IUnion
{
    public override bool CanConvert(Type objectType) => objectType == typeof(T) || objectType == typeof(T?);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(serializer);

        if (reader.TokenType == JsonToken.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonToken.StartObject)
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        // Advance to the first property or EndObject token
        if (!reader.Read())
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        object? result = null;
        string? firstPropertyName = null;

        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }

            string? propertyName = (string?)reader.Value;
            firstPropertyName ??= propertyName;

            if (result is null
                && propertyName is not null
                && ExternallyDiscriminatedUnion<T>.CasesByName.TryGetValue(propertyName, out var caseInfo))
            {
                // Advance to the value token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }

                object? value = serializer.Deserialize(reader, caseInfo.CaseType);
                if (value is null)
                {
                    // Null is not a valid union case
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }

                result = caseInfo.Factory(value);

                // Advance past the value token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }
            }
            else
            {
                // Skip the property if it is an unknown case or we have already found a matching case
                reader.Skip();

                // Read to the next property or EndObject token
                if (!reader.Read())
                {
                    JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
                }
            }
        }

        return result ?? CreateUnknownCase(firstPropertyName);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(serializer);

        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        if (value is not T union)
        {
            ThrowHelper.ThrowInvalidOperationException($"Cannot serialize value of type '{value.GetType().FullName}' as union type '{typeof(T).FullName}'.");
            return;
        }

        object? innerValue = union.Value;
        if (innerValue is not null)
        {
            writer.WriteStartObject();

            if (innerValue is UnknownCase)
            {
                // Support round-trip serialization of the unknown case by serializing it as an empty object
                writer.WriteEndObject();
                return;
            }

            Type? innerType = innerValue.GetType();
            do
            {
                if (ExternallyDiscriminatedUnion<T>.CasesByType.TryGetValue(innerType, out var caseInfo))
                {
                    writer.WritePropertyName(caseInfo.CaseName);

                    serializer.Serialize(writer, innerValue, caseInfo.CaseType);

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

    private static T CreateUnknownCase(string? caseName = null)
    {
        if (ExternallyDiscriminatedUnion<T>.UnknownCaseFactory is null)
        {
            JsonDiscriminatedUnionConverter.ThrowUnknownUnionCase(typeof(T), caseName);
        }

        return ExternallyDiscriminatedUnion<T>.UnknownCaseFactory(UnknownCase.Value);
    }
}

internal static class JsonDiscriminatedUnionConverter
{
    [DoesNotReturn]
    public static void ThrowInvalidUnionJson(Type unionType)
        => throw new JsonSerializationException($"Invalid JSON for union type '{unionType.FullName}'.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCaseType(Type unionType, Type? caseType)
        => throw new JsonSerializationException(caseType is not null
            ? $"Union type '{unionType.FullName}' does not have a case with type '{caseType.FullName}'."
            : $"Union type '{unionType.FullName}' may not contain a null value.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCase(Type unionType, string? caseName)
        => throw new JsonSerializationException(caseName is not null
            ? $"Union type '{unionType.FullName}' does not have a case '{caseName}'."
            : $"Union type '{unionType.FullName}' does not have an unknown case.");
}
