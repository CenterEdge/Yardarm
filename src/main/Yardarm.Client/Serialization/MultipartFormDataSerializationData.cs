using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Yardarm.Client.Internal;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Provides <see cref="ISerializationData"/> for the <see cref="MultipartFormDataSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of schema to be serialized.</typeparam>
    public class MultipartFormDataSerializationData<T> : ISerializationData
    {
        /// <summary>
        /// <see cref="MultipartPropertyInfo{T}"/> settings for each property of the schema, keyed by property name.
        /// </summary>
        private readonly Dictionary<string, MultipartPropertyInfo<T>> _properties;

        public IEnumerable<MultipartPropertyInfo<T>> Properties => _properties.Values;

        public MultipartFormDataSerializationData(params MultipartPropertyInfo<T>[] properties)
        {
            ThrowHelper.ThrowIfNull(properties);

            _properties = properties.ToDictionary(static p => p.PropertyName);
        }

        public bool TryGetProperty(string propertyKey, [NotNullWhen(true)] out MultipartPropertyInfo<T>? encoding) =>
            _properties.TryGetValue(propertyKey, out encoding);
    }
}
