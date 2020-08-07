using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorFactory<TElement>
        where TElement : IOpenApiSerializable
    {
        ITypeGenerator Create(LocatedOpenApiElement<TElement> element);
    }
}
