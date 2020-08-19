using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry
    {
        ITypeGenerator Get<T>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement;
    }
}
