using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public class NoopTypeGeneratorFactory<T, TGeneratorCategory> : ITypeGeneratorFactory<T, TGeneratorCategory>
        where T : IOpenApiElement
    {
        public ITypeGenerator Create(ILocatedOpenApiElement<T> element, ITypeGenerator? parent) =>
            new NoopTypeGenerator<T>(parent);
    }
}
