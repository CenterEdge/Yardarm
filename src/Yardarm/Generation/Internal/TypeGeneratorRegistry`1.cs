using System;
using System.Collections.Concurrent;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry<TElement> : ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiSerializable
    {
        private readonly ITypeGeneratorFactory<TElement> _factory;

        private readonly ConcurrentDictionary<LocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
            new ConcurrentDictionary<LocatedOpenApiElement<TElement>, ITypeGenerator>(new LocatedElementEqualityComparer<TElement>());

        public TypeGeneratorRegistry(ITypeGeneratorFactory<TElement> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public ITypeGenerator Get(LocatedOpenApiElement<TElement> element) =>
            _registry.GetOrAdd(element, _factory.Create);
    }
}
