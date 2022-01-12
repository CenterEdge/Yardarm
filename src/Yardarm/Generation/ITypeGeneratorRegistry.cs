using System;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGeneratorRegistry
    {
        ITypeGenerator Get(ILocatedOpenApiElement element, Type generatorCategory);

        public ITypeGenerator Get<T>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement =>
            Get<T, PrimaryGeneratorCategory>(element);

        ITypeGenerator Get<T, TGeneratorCategory>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement;
    }
}
