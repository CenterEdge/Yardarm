using System;
using System.Collections.Concurrent;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry<TElement> : ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiElement
    {
        private readonly ITypeGeneratorRegistry _mainRegistry;
        private readonly ITypeGeneratorFactory<TElement> _factory;

        private readonly ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
            new ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator>(new LocatedElementEqualityComparer<TElement>());

        public TypeGeneratorRegistry(ITypeGeneratorRegistry mainRegistry, ITypeGeneratorFactory<TElement> factory)
        {
            _mainRegistry = mainRegistry ?? throw new ArgumentNullException(nameof(mainRegistry));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element) =>
            _registry.GetOrAdd(element, key =>
                _factory.Create(key, key.Parent != null ? _mainRegistry.Get(key.Parent) : null));
    }
}
