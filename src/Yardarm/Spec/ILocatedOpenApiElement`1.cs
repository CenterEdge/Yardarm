using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public interface ILocatedOpenApiElement<out T> : ILocatedOpenApiElement
        where T : IOpenApiElement
    {
        new T Element { get; }
    }
}
