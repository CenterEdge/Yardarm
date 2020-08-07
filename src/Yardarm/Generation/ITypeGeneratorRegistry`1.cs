using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiSerializable
    {
        public ITypeGenerator Get(LocatedOpenApiElement<TElement> schemaElement);
    }
}
