using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Provides <see cref="ISerializationData"/> for the <see cref="MultipartFormDataSerializer"/>.
    /// </summary>
    public class MultipartFormDataSerializationData : ISerializationData
    {
        /// <summary>
        /// <see cref="MultipartEncoding"/> settings for each property of the schema, keyed by property name.
        /// </summary>
        private readonly Dictionary<string, MultipartEncoding> _encodings;

        public MultipartFormDataSerializationData(params (string key, MultipartEncoding encoding)[] encodings)
        : this(encodings?.ToDictionary(static p => p.key, static p => p.encoding)
            ?? throw new ArgumentNullException(nameof(encodings)))
        {
        }

        public MultipartFormDataSerializationData(Dictionary<string, MultipartEncoding> encodings)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(encodings);
#else
            if (encodings is null)
            {
                throw new ArgumentNullException(nameof(encodings));
            }
#endif

            _encodings = encodings;
        }

        public bool TryGetEncoding(string propertyKey, [NotNullWhen(true)] out MultipartEncoding? encoding) =>
            _encodings.TryGetValue(propertyKey, out encoding);
    }
}
