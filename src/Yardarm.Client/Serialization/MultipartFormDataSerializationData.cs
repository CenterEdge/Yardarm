using System.Collections.Generic;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Provides serialization data for the <see cref="MultipartFormDataSerializer"/>.
    /// </summary>
    internal class MultipartFormDataSerializationData
    {
        /// <summary>
        /// <see cref="MultipartEncoding"/> settings for each property of the schema, keyed by property name.
        /// </summary>
        public Dictionary<string, MultipartEncoding> Encoding { get; } = new Dictionary<string, MultipartEncoding>();
    }
}
