using System;
using System.Net.Http;
using RootNamespace.Authentication;

namespace RootNamespace.Requests;

/// <summary>
/// Base class for operation requests.
/// </summary>
public abstract class OperationRequest : IOperationRequest
{
    /// <inheritdoc />
    public IAuthenticator? Authenticator { get; set; }

    /// <inheritdoc />
    public bool EnableResponseStreaming { get; set; }

    /// <summary>
    /// The HTTP method of the request.
    /// </summary>
    protected abstract HttpMethod Method { get; }

    /// <summary>
    /// Add headers to the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <param name="requestMessage">Request message.</param>
    protected virtual void AddHeaders(BuildRequestContext context, HttpRequestMessage requestMessage)
    {
    }

    /// <summary>
    /// Create the content of the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <returns><see cref="HttpContent"/> or <c>null</c> if no content.</returns>
    protected virtual HttpContent? BuildContent(BuildRequestContext context) => null;

    /// <summary>
    /// Builds the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
    public virtual HttpRequestMessage BuildRequest(BuildRequestContext context)
    {
        var requestMessage = new HttpRequestMessage(Method, BuildUri(context));
        AddHeaders(context, requestMessage);
        requestMessage.Content = BuildContent(context);
        return requestMessage;
    }

    /// <summary>
    /// Create the URI of the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <returns><see cref="Uri"/> of the request.</returns>
    protected abstract Uri BuildUri(BuildRequestContext context);
}
