using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry<TElement, TGeneratorCategory> : ITypeGeneratorRegistry<TElement, TGeneratorCategory>
        where TElement : IOpenApiElement
    {
        private readonly ITypeGeneratorRegistry _mainRegistry;
        private readonly ITypeGeneratorFactory<TElement, TGeneratorCategory> _factory;
        private readonly OpenApiDocument _document;

        private readonly ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
            new(new LocatedElementEqualityComparer<TElement>());
        private readonly Func<ILocatedOpenApiElement<TElement>, ITypeGenerator> _createTypeGenerator;

        public TypeGeneratorRegistry(ITypeGeneratorRegistry mainRegistry, ITypeGeneratorFactory<TElement, TGeneratorCategory> factory,
            OpenApiDocument document)
        {
            ArgumentNullException.ThrowIfNull(mainRegistry);
            ArgumentNullException.ThrowIfNull(factory);
            ArgumentNullException.ThrowIfNull(document);

            _mainRegistry = mainRegistry;
            _factory = factory;
            _document = document;

            _createTypeGenerator = CreateTypeGenerator;
        }

        public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element) =>
            _registry
                .GetOrAdd(element, _createTypeGenerator);

        public IEnumerable<ITypeGenerator> GetAll() => _registry.Values;

        private ITypeGenerator CreateTypeGenerator(ILocatedOpenApiElement<TElement> element)
        {
            if (LocatedElementEqualityComparer<TElement>.GetIsReferenceEqualDefault() &&
                element.Element is IOpenApiReferenceable referenceable && referenceable.Reference != null)
            {
                // When making the new type generator with the factory for a reference, we must ensure
                // that we are using the referenced component path for the ILocatedOpenApiElement.

                var referencedElement = (TElement)_document.ResolveReference(referenceable.Reference);
                element = LocatedOpenApiElement.CreateRoot<TElement>(referencedElement, referenceable.Reference.Id);
            }

            return _factory.Create(element, element.Parent != null ? _mainRegistry.Get(element.Parent, typeof(TGeneratorCategory)) : null);
        }
    }
}
