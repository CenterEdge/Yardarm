using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry<in TElement>
        where TElement : IOpenApiElement
    {
        public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element);
    }
}
