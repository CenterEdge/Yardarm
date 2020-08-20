using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Yardarm.Spec
{
    /// <summary>
    /// Placeholder to represent any unknown response returned within an
    /// <see cref="OpenApiResponses"/>. This can be used to lookup the
    /// class inherited from UnknownResponse for a particular response set.
    /// </summary>
    public class OpenApiUnknownResponse : OpenApiResponse
    {
        public const string Key = "unknown";
    }
}
