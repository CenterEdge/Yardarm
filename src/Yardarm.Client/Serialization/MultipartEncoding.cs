using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Defines how to serialize multipart/form-data properties.
    /// </summary>
    public class MultipartEncoding
    {
        public IReadOnlyCollection<string> MediaTypes { get; }

        public MultipartEncoding(params string[] mediaTypes)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(mediaTypes);
#else
            if (mediaTypes is null)
            {
                throw new ArgumentNullException(nameof(mediaTypes));
            }
#endif

            MediaTypes = new ReadOnlyCollection<string>(mediaTypes);
        }

        public static implicit operator MultipartEncoding(string mediaType) =>
            new(mediaType);
    }
}
