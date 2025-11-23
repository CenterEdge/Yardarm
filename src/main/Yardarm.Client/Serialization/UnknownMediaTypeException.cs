using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

/// <summary>
/// A response returned an unknown status code.
/// </summary>
public class UnknownMediaTypeException(string? mediaType, HttpContent? httpContent)
    : Exception($"Unknown media type '{mediaType}'.")
{
    public HttpContent? HttpContent { get; } = httpContent;

    public string? MediaType { get; } = mediaType;

    public UnknownMediaTypeException(string? mediaType)
        : this(mediaType, null)
    {
    }
}
