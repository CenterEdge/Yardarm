using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace RootNamespace.Serialization.Json
{
    internal static class JsonHelpers
    {
        public static string? GetDiscriminator(ref Utf8JsonReader reader, ReadOnlySpan<byte> utf8PropertyName)
        {
            // Clone the reader so we don't mutate the original
            Utf8JsonReader readerClone = reader;

            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                ThrowJsonException();
            }

            // Read the property name
            if (!readerClone.Read())
            {
                ThrowJsonException();
            }
            while (readerClone.TokenType != JsonTokenType.EndObject)
            {
                if (readerClone.TokenType != JsonTokenType.PropertyName)
                {
                    ThrowJsonException();
                }

                if (readerClone.ValueTextEquals(utf8PropertyName))
                {
                    // Read the value
                    if (!readerClone.Read())
                    {
                        readerClone.Read();
                    }

                    return readerClone.GetString();
                }

                // Read the value
                if (!readerClone.Read())
                {
                    ThrowJsonException();
                }

                if (readerClone.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    // Skip the array or object to the next property name or end of the object
                    // This leaves us on the end of the object or array
                    if (!readerClone.TrySkip())
                    {
                        ThrowJsonException();
                    }
                }

                // Skip the value to the next property name or end of the object
                if (!readerClone.Read())
                {
                    ThrowJsonException();
                }
            }

            return null;
        }

        [DoesNotReturn]
        public static void ThrowJsonException()
        {
            throw new JsonException();
        }
    }
}
