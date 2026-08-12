using Microsoft.OpenApi;
using System.Collections.Generic;
using Yardarm.Spec;

namespace Yardarm.Generation;

// ReSharper disable once UnusedTypeParameter
public interface ITypeGeneratorRegistry<in TElement>
    where TElement : IOpenApiElement
{
    public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element);

    IEnumerable<ITypeGenerator> GetAll();
}
