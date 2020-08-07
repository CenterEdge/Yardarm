using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Helpers;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry<TElement> : ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiSerializable
    {
        private readonly ITypeGeneratorFactory<TElement> _factory;

        private readonly Dictionary<LocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
            new Dictionary<LocatedOpenApiElement<TElement>, ITypeGenerator>(new LocatedElementEqualityComparer<TElement>());

        public TypeGeneratorRegistry(ITypeGeneratorFactory<TElement> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public ITypeGenerator Get(LocatedOpenApiElement<TElement> element)
        {
            if (!_registry.TryGetValue(element, out var generator))
            {
                generator = _factory.Create(element);

                _registry[element] = generator;
            }

            return generator;
        }
    }
}
