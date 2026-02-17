using System;
using System.Net.Http;
using RootNamespace.Authentication;

#if NET5_0_OR_GREATER
using System.Collections.Generic;
#endif

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

#if NET5_0_OR_GREATER
    private HttpRequestOptions? _options;

    /// <inheritdoc />
    public HttpRequestOptions Options => _options ??= new();
#endif

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

#if NET5_0_OR_GREATER
        ApplyOptions(requestMessage);
#endif

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

#if NET5_0_OR_GREATER
    private void ApplyOptions(HttpRequestMessage requestMessage)
    {
        if (_options is HttpRequestOptions options)
        {
            var destination = (IDictionary<string, object?>)requestMessage.Options;

            foreach (KeyValuePair<string, object?> option in options)
            {
                destination[option.Key] = option.Value;
            }
        }
    }
#endif
}
