using Microsoft.OpenApi.Models;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public interface ISerializerSelector
    {
        SerializerDescriptor? Select(ILocatedOpenApiElement<OpenApiMediaType> mediaType);
    }
}
