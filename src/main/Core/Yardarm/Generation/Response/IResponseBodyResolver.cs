using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response;

/// <summary>
/// Determines the body type of responses, including whether the body is streamed.
/// </summary>
public interface IResponseBodyResolver
{
    /// <summary>
    /// Resolves the body of a response.
    /// </summary>
    /// <returns>The body, or <c>null</c> if the response has no body that can be deserialized.</returns>
    ResponseBodyInfo? Resolve(ILocatedOpenApiElement<IOpenApiResponse> response);
}
