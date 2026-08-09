using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation;

// ReSharper disable once UnusedTypeParameter
public interface ITypeGeneratorFactory<in TElement>
    where TElement : IOpenApiElement
{
    /// <summary>
    /// Priority of this factory relative to other factories. Factories with a lower priority
    /// will be tried first. Factories with an equal priority will be tried in the order they
    /// were registered. Default is 0.
    /// </summary>
    /// <remarks>
    /// Extensions are registered before built-in factories, so with an equal priority an extension
    /// factory will take precedence over built-in factories.
    /// </remarks>
    public int Priority => 0;

    /// <summary>
    /// Create a type generator for the given element.
    /// </summary>
    /// <param name="element">Element.</param>
    /// <param name="parent">Parent element.</param>
    /// <returns>
    /// A type generator, or <c>null</c> if no type is created by this factory and the
    /// next type generator should be tried.
    /// </returns>
    ITypeGenerator? Create(ILocatedOpenApiElement<TElement> element, ITypeGenerator? parent);
}
