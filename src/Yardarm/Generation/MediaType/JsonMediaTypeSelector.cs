using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class JsonMediaTypeSelector : IMediaTypeSelector
    {
        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            requestBody.Element.Content?
                .Where(p => p.Key == "application/json")
                .Select(p => requestBody.CreateChild(p.Value, p.Key))
                .FirstOrDefault();

        public ILocatedOpenApiElement<OpenApiMediaType>? Select(ILocatedOpenApiElement<OpenApiResponse> response) =>
            response.Element.Content?
                .Where(p => p.Key == "application/json")
                .Select(p => response.CreateChild(p.Value, p.Key))
                .FirstOrDefault();
    }
}
