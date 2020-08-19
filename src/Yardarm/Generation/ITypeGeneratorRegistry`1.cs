using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiElement
    {
        public ITypeGenerator Get(LocatedOpenApiElement<TElement> element);
    }
}
