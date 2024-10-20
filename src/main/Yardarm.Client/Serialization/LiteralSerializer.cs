using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public static class LiteralSerializer
    {
        public static string Serialize<T>(T value, string? format = null)
        {
            if (value is null)
            {
                return "";
            }

            // These if expressions are elided by JIT for value types to be only the specific branch
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
            {
                return (bool)(object)value ? "true" : "false";
            }
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                var dateTime = (DateTime)(object)value;

                return format switch
                {
                    "date" or "full-date" => dateTime.ToString("yyyy-MM-dd"),
                    _ => dateTime.ToString("O")
                };
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                var dateTime = (DateTimeOffset)(object)value;

                return format switch
                {
                    "date" or "full-date" => dateTime.ToString("yyyy-MM-dd"),
                    _ => dateTime.ToString("O")
                };
            }
            if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
            {
                var timeSpan = (TimeSpan)(object)value;
                return timeSpan.ToString("c");
            }
            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                // Optimization for GUIDs, avoids loading CultureInfo.InvariantCulture which has no effect on GUID formatting
                var guid = (Guid)(object)value;
                return guid.ToString();
            }

            if (value is IFormattable)
            {
                return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture); // constrained call avoiding boxing for value types
            }

            return value.ToString() ?? "";
        }

#if NET6_0_OR_GREATER

        public static bool TrySerialize<T>(T value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
        {
            if (value is null)
            {
                goto failed;
            }

            // These if expressions are elided by JIT for value types to be only the specific branch
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
            {
                var boolString = (bool)(object)value ? "true" : "false";
                if (boolString.TryCopyTo(destination))
                {
                    charsWritten = boolString.Length;
                    return true;
                }

                goto failed;
            }
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                var dateTime = (DateTime)(object)value;

                return format switch
                {
                    "date" or "full-date" => dateTime.TryFormat(destination, out charsWritten, format: "yyyy-MM-dd"),
                    _ => dateTime.TryFormat(destination, out charsWritten, format: "O")
                };
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                var dateTime = (DateTimeOffset)(object)value;

                return format switch
                {
                    "date" or "full-date" => dateTime.TryFormat(destination, out charsWritten, format: "yyyy-MM-dd"),
                    _ => dateTime.TryFormat(destination, out charsWritten, format: "O")
                };
            }
            if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
            {
                var timeSpan = (TimeSpan)(object)value;
                return timeSpan.TryFormat(destination, out charsWritten, format: "c");
            }
            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                // Optimization for GUIDs, avoids loading CultureInfo.InvariantCulture which has no effect on GUID formatting
                var guid = (Guid)(object)value;
                return guid.TryFormat(destination, out charsWritten);
            }

            string strValue;
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
                strValue = value.ToString() ?? "";
            }

            if (strValue.TryCopyTo(destination))
            {
                charsWritten = strValue.Length;
                return true;
            }

        failed:
            charsWritten = 0;
            return false;
        }

#endif

        [return: NotNullIfNotNull(nameof(value))]
        public static T Deserialize<T>(string? value, string? format = null)
        {
            if (value is null)
            {
                return default!;
            }

            // These if expressions are elided by JIT for value types to be only the specific branch
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
            {
                return (T)(object)bool.Parse(value);
            }
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                return (T)(object)int.Parse(value);
            }
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?))
            {
                return (T)(object)uint.Parse(value);
            }
            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
            {
                return (T)(object)long.Parse(value);
            }
            if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?))
            {
                return (T)(object)ulong.Parse(value);
            }
            if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
            {
                return (T)(object)short.Parse(value);
            }
            if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?))
            {
                return (T)(object)ushort.Parse(value);
            }
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
            {
                return (T)(object)byte.Parse(value);
            }
            if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?))
            {
                return (T)(object)sbyte.Parse(value);
            }
            if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
            {
                return (T)(object)float.Parse(value);
            }
            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
            {
                return (T)(object)double.Parse(value);
            }
            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
            {
                return (T)(object)decimal.Parse(value);
            }
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                return (T)(object)(format switch
                {
                    "date" or "full-date" => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => DateTime.Parse(value, CultureInfo.InvariantCulture)
                });
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                return (T)(object)(format switch
                {
                    "date" or "full-date" => DateTimeOffset.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture)
                });
            }
            if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
            {
                return (T)(object)(format switch
                {
                    "partial-time" or "date-span" => TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture),
                    _ => TimeSpan.Parse(value, CultureInfo.InvariantCulture)
                });
            }
            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                return (T)(object)Guid.Parse(value);
            }
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value)!;
            }

            // Reference types aren't elided
            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            if (typeof(T) == typeof(Uri))
            {
                if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
                {
                    return (T)(object)uri;
                }

                ThrowHelper.ThrowFormatException("Invalid URI format.");
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
