using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Serialization
{
    public interface ISerializerSelector
    {
        SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<IOpenApiMediaType> mediaType);
    }
}
