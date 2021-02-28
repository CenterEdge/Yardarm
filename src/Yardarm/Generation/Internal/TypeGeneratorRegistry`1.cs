using System;
using System.Collections.Concurrent;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry<TElement> : ITypeGeneratorRegistry<TElement>
        where TElement : IOpenApiElement
    {
        private readonly ITypeGeneratorRegistry _mainRegistry;
        private readonly ITypeGeneratorFactory<TElement> _factory;
        private readonly OpenApiDocument _document;

        private readonly ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
            new ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator>(new LocatedElementEqualityComparer<TElement>());

        public TypeGeneratorRegistry(ITypeGeneratorRegistry mainRegistry, ITypeGeneratorFactory<TElement> factory,
            OpenApiDocument document)
        {
            _mainRegistry = mainRegistry ?? throw new ArgumentNullException(nameof(mainRegistry));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element) =>
            _registry.GetOrAdd(element, key =>
            {
                if (LocatedElementEqualityComparer<TElement>.GetIsReferenceEqualDefault() &&
                    key.Element is IOpenApiReferenceable referenceable && referenceable.Reference != null)
                {
                    // When making the new type generator with the factory for a reference, we must ensure
                    // that we are using the referenced component path for the ILocatedOpenApiElement.

                    var referencedElement = (TElement) _document.ResolveReference(referenceable.Reference);
                    key = LocatedOpenApiElement.CreateRoot<TElement>(referencedElement, referenceable.Reference.Id);
                }

                return _factory.Create(key, key.Parent != null ? _mainRegistry.Get(key.Parent) : null);
            });
    }
}
