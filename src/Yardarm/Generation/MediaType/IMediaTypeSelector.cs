using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    /// <summary>
    /// Selects the media type which is used to generate models.
    /// Returns null if no valid media types are found.
    /// </summary>
    public interface IMediaTypeSelector
    {
        /// <summary>
        /// Selects the media type which is used to generate models.
        /// Returns null if no valid media types are found.
        /// </summary>
        LocatedOpenApiElement<OpenApiMediaType>? Select(LocatedOpenApiElement<OpenApiRequestBody> requestBody);

        /// <summary>
        /// Selects the media type which is used to generate models.
        /// Returns null if no valid media types are found.
        /// </summary>
        LocatedOpenApiElement<OpenApiMediaType>? Select(LocatedOpenApiElement<OpenApiResponse> response);
    }
}
