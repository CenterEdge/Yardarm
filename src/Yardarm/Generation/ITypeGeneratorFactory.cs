using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorFactory<in TElement>
        where TElement : IOpenApiElement
    {
        ITypeGenerator Create(ILocatedOpenApiElement<TElement> element);
    }
}
