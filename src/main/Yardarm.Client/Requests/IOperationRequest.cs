using RootNamespace.Authentication;
using RootNamespace.Responses;

#if NET5_0_OR_GREATER
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
#endif

namespace RootNamespace.Requests;

public interface IOperationRequest
{
    /// <summary>
    /// Optionally override the default authenticator for this request.
    /// </summary>
    public IAuthenticator? Authenticator { get; set; }

    /// <summary>
    /// If true, the request will return after response headers are received and the response body will be streamed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is particularly useful for large response bodies. However, when using response streaming the response body
    /// will not be buffered and may only be read once.
    /// </para>
    /// <para>
    /// Additionally, it is imperative that <see cref="IOperationResponse.Dispose" /> be called when the response is no
    /// longer needed to avoid leaking connections. This is particularly important for .NET 4.x consumers.
    /// </para>
    /// </remarks>
    public bool EnableResponseStreaming { get; set; }

#if NET5_0_OR_GREATER
    /// <summary>
    /// Options to apply to the <see cref="HttpRequestMessage"/>.
    /// </summary>
    // This includes a DIM for backward compatibility, it should normally be unused.
    public HttpRequestOptions Options => OperationRequestExtensions.OptionsTable.GetValue(this, static _ => []);
#endif
}

#if NET5_0_OR_GREATER
internal static class OperationRequestExtensions
{
    private static ConditionalWeakTable<IOperationRequest, HttpRequestOptions>? s_optionsTable = null;

    public static ConditionalWeakTable<IOperationRequest, HttpRequestOptions> OptionsTable =>
        s_optionsTable ?? Interlocked.CompareExchange(ref s_optionsTable, [], null) ?? s_optionsTable;
}
#endif
