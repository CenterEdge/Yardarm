using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Serialization
{
    public interface ISerializerSelector
    {
        SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<OpenApiMediaType> mediaType);
    }
}
