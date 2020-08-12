using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorFactory<TElement>
        where TElement : IOpenApiSerializable
    {
        ITypeGenerator Create(LocatedOpenApiElement<TElement> element);
    }
}
