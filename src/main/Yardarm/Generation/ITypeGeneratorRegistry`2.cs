using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    // ReSharper disable once UnusedTypeParameter
    public interface ITypeGeneratorRegistry<in TElement, TGeneratorCategory>
        where TElement : IOpenApiElement
    {
        public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element);

        IEnumerable<ITypeGenerator> GetAll();
    }
}
