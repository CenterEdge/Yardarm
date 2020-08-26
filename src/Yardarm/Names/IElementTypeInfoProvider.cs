using Yardarm.Spec;

namespace Yardarm.Names
{
    public interface IElementTypeInfoProvider
    {
        YardarmTypeInfo Get(ILocatedOpenApiElement element);
    }
}
