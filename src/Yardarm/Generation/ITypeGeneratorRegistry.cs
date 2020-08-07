using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry
    {
        ITypeGenerator Get<T>(LocatedOpenApiElement<T> element)
            where T : IOpenApiSerializable;
    }
}
