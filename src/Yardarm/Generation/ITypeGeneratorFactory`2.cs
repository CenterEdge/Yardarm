using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    // ReSharper disable once UnusedTypeParameter
    public interface ITypeGeneratorFactory<in TElement, TGeneratorCategory>
        where TElement : IOpenApiElement
    {
        ITypeGenerator Create(ILocatedOpenApiElement<TElement> element, ITypeGenerator? parent);
    }
}
