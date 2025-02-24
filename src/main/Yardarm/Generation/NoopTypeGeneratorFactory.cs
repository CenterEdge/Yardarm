using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation;

internal class NoopTypeGeneratorFactory<T> : ITypeGeneratorFactory<T>
    where T : IOpenApiElement
{
    public ITypeGenerator Create(ILocatedOpenApiElement<T> element, ITypeGenerator? parent) =>
        new NoopTypeGenerator<T>(parent);
}
