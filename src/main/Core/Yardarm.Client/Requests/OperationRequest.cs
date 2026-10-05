using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

#if NET5_0_OR_GREATER
    private HttpRequestOptions? _options;

    /// <inheritdoc />
    public HttpRequestOptions Options => _options ??= new();
#endif

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
    /// Create the content of the HTTP request message asynchronously.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="HttpContent"/> or <c>null</c> if no content.</returns>
    /// <remarks>
    /// The default implementation forwards to <see cref="BuildContent"/>. This is only called by
    /// <see cref="BuildRequestWithAsyncContentAsync"/>.
    /// </remarks>
    protected virtual ValueTask<HttpContent?> BuildContentAsync(BuildRequestContext context,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return FromCanceled<HttpContent?>(cancellationToken);
        }

        return new ValueTask<HttpContent?>(BuildContent(context));
    }

    /// <summary>
    /// Builds the HTTP request message.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
    public virtual HttpRequestMessage BuildRequest(BuildRequestContext context)
    {
        var requestMessage = CreateRequestMessage(context);
        requestMessage.Content = BuildContent(context);
        return requestMessage;
    }

    /// <summary>
    /// Builds the HTTP request message asynchronously.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
    /// <remarks>
    /// The default implementation forwards to <see cref="BuildRequest"/>. Requests whose content must be built
    /// asynchronously override this method to call <see cref="BuildRequestWithAsyncContentAsync"/>.
    /// </remarks>
    public virtual ValueTask<HttpRequestMessage> BuildRequestAsync(BuildRequestContext context,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return FromCanceled<HttpRequestMessage>(cancellationToken);
        }

        return new ValueTask<HttpRequestMessage>(BuildRequest(context));
    }

    /// <summary>
    /// Builds the HTTP request message with content from <see cref="BuildContentAsync"/>.
    /// </summary>
    /// <param name="context">Context of the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
    protected async ValueTask<HttpRequestMessage> BuildRequestWithAsyncContentAsync(BuildRequestContext context,
        CancellationToken cancellationToken = default)
    {
        var requestMessage = CreateRequestMessage(context);
        try
        {
            requestMessage.Content = await BuildContentAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            requestMessage.Dispose();
            throw;
        }

        return requestMessage;
    }

    private HttpRequestMessage CreateRequestMessage(BuildRequestContext context)
    {
        var requestMessage = new HttpRequestMessage(Method, BuildUri(context));
        ApplyHttpVersion(requestMessage);

#if NET5_0_OR_GREATER
        ApplyOptions(requestMessage);
#endif

        AddHeaders(context, requestMessage);
        return requestMessage;
    }

    private static ValueTask<T> FromCanceled<T>(CancellationToken cancellationToken) =>
#if NET5_0_OR_GREATER
        ValueTask.FromCanceled<T>(cancellationToken);
#else
        new(Task.FromCanceled<T>(cancellationToken));
#endif

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
