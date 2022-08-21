using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    /// <summary>
    /// Stores <see cref="LocatedOpenApiElement"/> instances indexed by a random key. This assists with
    /// referencing the element from a <see cref="Microsoft.CodeAnalysis.SyntaxAnnotation"/>, which can
    /// only reference strings.
    /// </summary>
    public interface IOpenApiElementRegistry
    {
        ILocatedOpenApiElement<T> Get<T>(string key)
            where T : IOpenApiElement;

        bool TryGet<T>(string key, [MaybeNullWhen(false)] out ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement;

        string Add<T>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement;
    }
}
