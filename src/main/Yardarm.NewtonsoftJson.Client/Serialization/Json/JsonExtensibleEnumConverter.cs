using System;
using Newtonsoft.Json;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Json;

internal sealed class JsonExtensibleEnumConverter<T> : JsonConverter
    where T : struct, IExtensibleEnum<T>
{
    public override bool CanConvert(Type objectType) => objectType == typeof(T);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);

        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.String)
        {
            return ExtensibleEnum.Create<T>((string)reader.Value!);
        }

        throw new JsonSerializationException($"Unable to convert JSON to {typeof(T).FullName}.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNull();
        }
        else if (value is T typedValue)
        {
            writer.WriteValue(typedValue.Value);
        }
        else
        {
            throw new JsonSerializationException($"{value.GetType().FullName} is not of type {typeof(T).FullName}.");
        }
    }
}
