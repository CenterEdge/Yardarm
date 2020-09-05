using Microsoft.OpenApi.Models;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public interface ISerializerSelector
    {
        SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<OpenApiMediaType> mediaType);
    }
}
