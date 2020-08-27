using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry
    {
        ITypeGenerator Get(ILocatedOpenApiElement element);

        ITypeGenerator Get<T>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement;
    }
}
