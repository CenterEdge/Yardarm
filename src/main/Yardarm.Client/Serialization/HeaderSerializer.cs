using System.Collections.Generic;
using RootNamespace.Serialization.Literals;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public static class HeaderSerializer
    {
        public static string SerializePrimitive<T>(T value, string? format = null)
        {
            if (value == null)
            {
                return "";
            }

            if (value is string str)
            {
                // Short-circuit for strings
                return str;
            }

            return LiteralSerializer.Serialize(value, format);
        }

        public static string SerializeList<T>(IEnumerable<T>? list, string? format = null)
        {
            if (list == null)
            {
                return "";
            }

            return LiteralSerializer.JoinList(",", list, format);
        }

        public static T DeserializePrimitive<T>(IEnumerable<string> values, string? format = null)
        {
            // Rejoin the values from the header into a simple string
#if NET6_0_OR_GREATER
            string value = string.Join(',', values);
#else
            string value = string.Join(",", values);
#endif

            if (typeof(T) == typeof(string))
            {
                // Short-circuit for strings
                return (T)(object)value;
            }

            // We're not dealing with a list, so join the values back together
            return LiteralSerializer.Deserialize<T>(value, format);
        }

        public static List<T> DeserializeList<T>(IEnumerable<string> values, string? format = null)
        {
            ThrowHelper.ThrowIfNull(values);

            return LiteralSerializer.DeserializeList<T>(values, format);
        }
    }
}
