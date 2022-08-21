using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    /// <summary>
    /// A response returned an unknown status code.
    /// </summary>
    public class UnknownMediaTypeException : Exception
    {
        public HttpContent? HttpContent { get; }

        public string? MediaType { get; }

        public UnknownMediaTypeException(string? mediaType)
            : this(mediaType, null)
        {
        }

        public UnknownMediaTypeException(string? mediaType, HttpContent? httpContent)
            : base($"Unknown media type '{mediaType}'.")
        {
            MediaType = mediaType;
            HttpContent = httpContent;
        }
    }
}
