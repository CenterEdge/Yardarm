using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Json;

internal sealed class JsonExtensibleEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : JsonConverter<T>
    where T : struct, IExtensibleEnum<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();
            if (value is not null)
            {
                return ExtensibleEnum.Create<T>(value);
            }
        }

        throw new JsonException($"Unable to convert JSON to {typeof(T).FullName}.");
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.PropertyName)
        {
            string? value = reader.GetString();
            if (value is not null)
            {
                return ExtensibleEnum.Create<T>(value);
            }
        }

        throw new JsonException($"Unable to convert JSON property name to {typeof(T).FullName}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(value.Value);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WritePropertyName(value.Value);
    }
}
