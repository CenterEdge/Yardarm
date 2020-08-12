using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry
    {
        ITypeGenerator Get<T>(LocatedOpenApiElement<T> element)
            where T : IOpenApiSerializable;
    }
}
