using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using RootNamespace.Serialization;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Responses;

public abstract class OperationResponse : IOperationResponse
{
    public HttpResponseMessage Message { get; }

    public HttpStatusCode StatusCode => Message.StatusCode;

    public bool IsSuccessStatusCode => Message.IsSuccessStatusCode;

    protected ITypeSerializerRegistry TypeSerializerRegistry { get; }

    /// <summary>
    /// Create a new OperationResponse.
    /// </summary>
    /// <param name="statusCode">Response status code.</param>
    /// <param name="headers">Optional response headers.</param>
    /// <remarks>
    /// Primarily used to support mock responses for unit tests.
    /// </remarks>
    protected OperationResponse(HttpStatusCode statusCode, HttpResponseHeaders? headers = null)
        : this(CreateMessage(statusCode, headers))
    {
    }

    protected OperationResponse(HttpResponseMessage message)
        : this (message, Serialization.TypeSerializerRegistry.Instance)
    {
    }

    protected OperationResponse(HttpResponseMessage message, ITypeSerializerRegistry typeSerializerRegistry)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(typeSerializerRegistry);

        Message = message;
        TypeSerializerRegistry = typeSerializerRegistry;

        // ReSharper disable once VirtualMemberCallInConstructor
        ParseHeaders(message.Headers);
    }

    /// <summary>
    /// Called during construction to parse headers from the message into properties.
    /// </summary>
    protected virtual void ParseHeaders(HttpResponseHeaders headers)
    {
    }

    public virtual void Dispose()
    {
        Message.Dispose();
        GC.SuppressFinalize(this);
    }

    private static HttpResponseMessage CreateMessage(HttpStatusCode statusCode, HttpResponseHeaders? headers)
    {
        var message = new HttpResponseMessage(statusCode);

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }
        }

        return message;
    }
}
