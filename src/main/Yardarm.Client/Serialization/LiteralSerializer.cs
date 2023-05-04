using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class LiteralSerializer
    {
        public static LiteralSerializer Instance { get; } = new();

        private static readonly MethodInfo s_joinListMethod =
            ((Func<string, IEnumerable<string>, string, string>)Instance.JoinList<string>).GetMethodInfo().GetGenericMethodDefinition();

        public string Serialize<T>(T value, string? format = null)
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
                    "date" => dateTime.ToString("yyyy-MM-dd"),
                    _ => dateTime.ToString("O")
                };
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                var dateTime = (DateTimeOffset)(object)value;

                return format switch
                {
                    "date" => dateTime.ToString("yyyy-MM-dd"),
                    _ => dateTime.ToString("O")
                };
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString() ?? "";
        }

        [return: NotNullIfNotNull(nameof(value))]
        public T Deserialize<T>(string? value, string? format = null)
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
                    "date" => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => DateTime.Parse(value, CultureInfo.InvariantCulture)
                });
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                return (T)(object)(format switch
                {
                    "date" => DateTimeOffset.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture)
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

        public string JoinList(string separator, object list, Type itemType, string? format = null)
        {
            MethodInfo joinList = s_joinListMethod.MakeGenericMethod(itemType);

            return (string)joinList.Invoke(this, new object?[] {separator, list, format})!;
        }

        public string JoinList<T>(string separator, IEnumerable<T> list, string? format = null) =>
            string.Join(separator, list
                .Select(p => Serialize(p, format)));

        public List<T> DeserializeList<T>(IEnumerable<string> values, string? format = null) =>
            new List<T>(values.Select(p => Deserialize<T>(p, format)));
    }
}
