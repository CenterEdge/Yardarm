using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiSerializable
    {
        public ITypeGenerator Get(LocatedOpenApiElement<TElement> element);
    }
}
