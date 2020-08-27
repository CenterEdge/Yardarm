using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public class NoopTypeGeneratorFactory<T> : ITypeGeneratorFactory<T>
        where T : IOpenApiElement
    {
        public ITypeGenerator Create(ILocatedOpenApiElement<T> element, ITypeGenerator? parent) =>
            new NoopTypeGenerator<T>(parent);
    }
}
