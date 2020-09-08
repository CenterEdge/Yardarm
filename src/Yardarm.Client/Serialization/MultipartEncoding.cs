using System;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Defines how to serialize multipart/form-data properties.
    /// </summary>
    public class MultipartEncoding
    {
        public string MediaType { get; set; }

        public MultipartEncoding(string mediaType)
        {
            MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        }

        public static implicit operator MultipartEncoding(string mediaType) =>
            new MultipartEncoding(mediaType);
    }
}
