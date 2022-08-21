using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry<in TElement> : ITypeGeneratorRegistry<TElement, PrimaryGeneratorCategory>
        where TElement : IOpenApiElement
    {
    }
}
