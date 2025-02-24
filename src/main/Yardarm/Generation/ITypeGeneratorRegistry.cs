using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation;

public interface ITypeGeneratorRegistry
{
    ITypeGenerator Get(ILocatedOpenApiElement element, string? generatorCategory = null);

    ITypeGenerator Get<T>(ILocatedOpenApiElement<T> element, string? generatorCategory = null)
        where T : IOpenApiElement;
}
