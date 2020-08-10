using System.Linq;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.MediaType
{
    public class JsonMediaTypeSelector : IMediaTypeSelector
    {
        public LocatedOpenApiElement<OpenApiMediaType>? Select(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            requestBody.Element.Content?
                .Where(p => p.Key == "application/json")
                .Select(p => requestBody.CreateChild(p.Value, p.Key))
                .FirstOrDefault();

        public LocatedOpenApiElement<OpenApiMediaType>? Select(LocatedOpenApiElement<OpenApiResponse> response) =>
            response.Element.Content?
                .Where(p => p.Key == "application/json")
                .Select(p => response.CreateChild(p.Value, p.Key))
                .FirstOrDefault();
    }
}
