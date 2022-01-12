using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorFactory<in TElement> : ITypeGeneratorFactory<TElement, PrimaryGeneratorCategory>
        where TElement : IOpenApiElement
    {
    }
}
