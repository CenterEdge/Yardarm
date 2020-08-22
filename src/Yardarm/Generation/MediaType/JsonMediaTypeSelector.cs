using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class JsonMediaTypeSelector : IMediaTypeSelector
    {
        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            requestBody
                .GetMediaTypes()
                .FirstOrDefault(p => p.Key == "application/json");

        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiResponse> response) =>
            response
                .GetMediaTypes()
                .FirstOrDefault(p => p.Key == "application/json");
    }
}
