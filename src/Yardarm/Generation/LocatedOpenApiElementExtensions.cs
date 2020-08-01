using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public static class LocatedOpenApiElementExtensions
    {
        public static LocatedOpenApiElement<T> CreateRoot<T>(this T rootItem, string key)
            where T : IOpenApiSerializable =>
            LocatedOpenApiElement.CreateRoot(rootItem, key);
    }
}
