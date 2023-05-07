using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json
{
    // This variant is only required until https://github.com/dotnet/runtime/issues/81833
    // has been fixed.

    /// <summary>
    /// Handles reading and writing date-only JSON to and from nullable <see cref="DateTime"/> properties.
    /// </summary>
    internal sealed class JsonNullableDateConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return JsonDateConverter.Read(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonDateConverter.Write(writer, value.Value);
            }
        }
    }
}
