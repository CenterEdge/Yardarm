using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class HeaderSerializer
    {
        public static HeaderSerializer Instance { get; } = new HeaderSerializer();

        public string Serialize<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(T value, bool explode)
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

            if (SerializationHelpers.IsEnumerable(typeof(T), out Type? itemType))
            {
                return LiteralSerializer.Instance.JoinList(",", value, itemType);
            }

            return LiteralSerializer.Instance.Serialize(value);
        }

        public T Deserialize<T>(IEnumerable<string> values, bool explode)
        {
            if (typeof(T) == typeof(string))
            {
                // Short-circuit for strings
                return (T)(object)string.Join(",", values);;
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (SerializationHelpers.IsList(typeof(T), out Type? itemType))
            {
                return (T) LiteralSerializer.Instance.DeserializeList(values, itemType);
            }

            // We're not dealing with a list, so join the values back together
            return LiteralSerializer.Instance.Deserialize<T>(string.Join(",", values));
        }
    }
}
