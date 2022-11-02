using System.Collections.Generic;
using System.Linq;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class HeaderSerializer
    {
        public static HeaderSerializer Instance { get; } = new(LiteralSerializer.Instance);

        private readonly LiteralSerializer _literalSerializer;

        public HeaderSerializer(LiteralSerializer literalSerializer)
        {
            ThrowHelper.ThrowIfNull(literalSerializer, nameof(literalSerializer));

            _literalSerializer = literalSerializer;
        }

        public string SerializePrimitive<T>(T value)
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

            return _literalSerializer.Serialize(value);
        }

        public string SerializeList<T>(IEnumerable<T>? list)
        {
            if (list == null)
            {
                return "";
            }

            return _literalSerializer.JoinList(",", list);
        }

        public T DeserializePrimitive<T>(IEnumerable<string> values)
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
            return _literalSerializer.Deserialize<T>(value);
        }

        public List<T> DeserializeList<T>(IEnumerable<string> values)
        {
            ThrowHelper.ThrowIfNull(values, nameof(values));

            return _literalSerializer.DeserializeList<T>(values);
        }
    }
}
