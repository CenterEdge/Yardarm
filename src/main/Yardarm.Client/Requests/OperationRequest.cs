using System;
using System.Net.Http;
using RootNamespace.Authentication;

namespace RootNamespace.Requests;

/// <summary>
/// Base class for operation requests.
/// </summary>
public abstract class OperationRequest : IOperationRequest
{
    // The default HTTP version to use for requests. This may be set during SDK generation, if not then it
    // will use the default for the target runtime.
    private static readonly Version s_defaultHttpVersion = new HttpRequestMessage().Version;
#if NET5_0_OR_GREATER
    private static readonly HttpVersionPolicy s_defaultHttpVersionPolicy = new HttpRequestMessage().VersionPolicy;
#endif

    /// <inheritdoc />
    public IAuthenticator? Authenticator { get; set; }

    /// <inheritdoc />
    public bool EnableResponseStreaming { get; set; }

    /// <summary>
    /// The HTTP method of the request.
    /// </summary>
    protected abstract HttpMethod Method { get; }

    /// <inheritdoc cref="HttpRequestMessage.Version"/>
    public Version HttpVersion { get; set; } = s_defaultHttpVersion;

#if NET5_0_OR_GREATER
    /// <inheritdoc cref="HttpRequestMessage.VersionPolicy"/>
    public HttpVersionPolicy HttpVersionPolicy { get; set; } = s_defaultHttpVersionPolicy;
#endif

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
        ApplyHttpVersion(requestMessage);
        AddHeaders(context, requestMessage);
        requestMessage.Content = BuildContent(context);
        return requestMessage;
    }

    protected void ApplyHttpVersion(HttpRequestMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.Version = HttpVersion;

#if NET5_0_OR_GREATER
        message.VersionPolicy = HttpVersionPolicy;
#endif
    }

    /// <summary>
    /// Create the URI of the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <returns><see cref="Uri"/> of the request.</returns>
    protected abstract Uri BuildUri(BuildRequestContext context);
}
