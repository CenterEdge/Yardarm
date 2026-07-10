using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// Wraps an inner converter for serializing and deserializing nullable value types.
/// </summary>
/// <param name="innerConverter">Inner converter for serializing and deserializing when not null.</param>
/// <remarks>
/// While System.Text.Json has built-in support for nullable value types, this converter is useful for overriding
/// the behavior, especially if there is another converter applied that overrides the behavior. This is included
/// in the client so it may be easily used by other extensions, such as the NodaTime extension. The Yardarm.SystemTextJson
/// extension doesn't currently use it directly.
/// </remarks>
internal sealed class NullableConverter<T>(JsonConverter<T> innerConverter) : JsonConverter<T?>
    where T : struct
{
    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return innerConverter.Read(ref reader, typeToConvert, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            innerConverter.Write(writer, value.Value, options);
        }
    }
}
