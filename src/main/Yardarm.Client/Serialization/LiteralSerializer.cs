using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class LiteralSerializer
    {
        public static LiteralSerializer Instance { get; } = new LiteralSerializer();

        private static readonly MethodInfo s_joinListMethod =
            ((Func<string, IEnumerable<string>, string, string>)Instance.JoinList<string>).GetMethodInfo().GetGenericMethodDefinition();

        public string Serialize<T>(T value, string? format = null)
        {
            if (value is null)
            {
                return "";
            }

            // These if expressions is elided by JIT for value types to be only the specific branch
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

            return TypeDescriptor.GetConverter(typeof(T))
                .ConvertToString(null, CultureInfo.InvariantCulture, value) ?? "";
        }

        [return: NotNullIfNotNull("value")]
        public T Deserialize<T>(string? value, string? format = null)
        {
            if (value is null)
            {
                return default!;
            }

            // These if expressions is elided by JIT for value types to be only the specific branch
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                return (T)(object)(format switch
                {
                    "date" or "full-date" => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => (DateTime) TypeDescriptor.GetConverter(typeof(DateTime))
                        .ConvertFromString(null, CultureInfo.InvariantCulture, value)!
                });
            }
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                return (T)(object)(format switch
                {
                    "date" or "full-date" => DateTimeOffset.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    _ => (DateTimeOffset) TypeDescriptor.GetConverter(typeof(DateTimeOffset))
                        .ConvertFromString(null, CultureInfo.InvariantCulture, value)!
                });
            }
            if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
            {
                return (T)(object)(format switch
                {
                    "partial-time" => TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture),
                    _ => (TimeSpan) TypeDescriptor.GetConverter(typeof(TimeSpan))
                        .ConvertFromString(null, CultureInfo.InvariantCulture, value)!
                });
            }

            return (T)TypeDescriptor.GetConverter(typeof(T))
                .ConvertFromString(null, CultureInfo.InvariantCulture, value)!;
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
