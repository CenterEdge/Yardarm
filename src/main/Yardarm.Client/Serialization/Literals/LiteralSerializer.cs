using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Literals
{
    /// <summary>
    /// Converts literals to strings for headers, query parameters, etc.
    /// </summary>
    public static class LiteralSerializer
    {
        public static string Serialize<T>(T value, string? format = null)
        {
            if (LiteralConverterRegistry.Instance.TryGet(out LiteralConverter<T>? converter))
            {
                return converter.Write(value, format);
            }

            if (value is IFormattable)
            {
                return ((IFormattable)value).ToString(format: null, CultureInfo.InvariantCulture); // constrained call avoiding boxing for value types
            }

            return value?.ToString() ?? "";
        }

#if NET6_0_OR_GREATER

        public static bool TrySerialize<T>(T value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
        {
            if (LiteralConverterRegistry.Instance.TryGet(out LiteralConverter<T>? converter))
            {
                return converter.TryWrite(value, format, destination, out charsWritten);
            }

            string? strValue = null;

            if (value is not null)
            {
                if (value is IFormattable)
                {
                    if (value is ISpanFormattable)
                    {
                        return ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format: default, provider: CultureInfo.InvariantCulture); // constrained call avoiding boxing for value types
                    }

                    strValue = ((IFormattable)value).ToString(format: null, CultureInfo.InvariantCulture); // constrained call avoiding boxing for value types
                }
                else
                {
                    strValue = value.ToString();
                }
            }

            if (strValue is null)
            {
                charsWritten = 0;
                return true;
            }
            if (strValue.TryCopyTo(destination))
            {
                charsWritten = strValue.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }

#endif

        [return: NotNullIfNotNull(nameof(value))]
        public static T Deserialize<T>(string? value, string? format = null)
        {
            if (LiteralConverterRegistry.Instance.TryGet(out LiteralConverter<T>? converter))
            {
                return converter.Read(value, format);
            }

            if (value is null)
            {
                return default!;
            }

            // Fallback logic to cover enums, which generally aren't explicitly registered
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value)!;
            }

            // Also check for nullable enums
            Type? underlyingType = Nullable.GetUnderlyingType(typeof(T));
            if (underlyingType is { IsEnum: true })
            {
                return (T)Enum.Parse(underlyingType, value);
            }

            ThrowHelper.ThrowInvalidOperationException($"Type '{typeof(T).FullName}' is not supported for deserialization by {nameof(LiteralSerializer)}.");
            return default!; // unreachable
        }

        public static string JoinList<T>(string separator, IEnumerable<T> list, string? format = null) =>
            string.Join(separator, list
                .Select(p => Serialize(p, format)));

        public static List<T> DeserializeList<T>(IEnumerable<string> values, string? format = null) =>
            new List<T>(values.Select(p => Deserialize<T>(p, format)));
    }
}
