using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal;

internal class TypeGeneratorRegistry<TElement> : ITypeGeneratorRegistry<TElement>
    where TElement : IOpenApiElement
{
    private static NoopTypeGeneratorFactory<TElement>? s_noopTypeGeneratorFactory;

    private readonly ITypeGeneratorRegistry _mainRegistry;
    private readonly ITypeGeneratorFactory<TElement>[] _factories;
    private readonly OpenApiDocument _document;
    private readonly string? _generatorCategory;

    private readonly ConcurrentDictionary<ILocatedOpenApiElement<TElement>, ITypeGenerator> _registry =
        new(new LocatedElementEqualityComparer<TElement>());

    public TypeGeneratorRegistry(
        ITypeGeneratorRegistry mainRegistry,
        OpenApiDocument document,
        IServiceProvider serviceProvider)
        : this(mainRegistry, document, serviceProvider, generatorCategory: null)
    {
    }

    public TypeGeneratorRegistry(
        ITypeGeneratorRegistry mainRegistry,
        OpenApiDocument document,
        IServiceProvider serviceProvider,
        [ServiceKey] string? generatorCategory)
    {
        ArgumentNullException.ThrowIfNull(mainRegistry);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _mainRegistry = mainRegistry;
        _document = document;
        _generatorCategory = generatorCategory;

        _factories =
        [
            .. serviceProvider.GetRequiredKeyedService<IEnumerable<ITypeGeneratorFactory<TElement>>>(generatorCategory)
                .Select((factory, index) => (factory, index))
                .OrderBy(p => p.factory.Priority)
                .ThenBy(p => p.index)
                .Select(p => p.factory)
        ];
    }

    public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element) =>
        _registry
            .GetOrAdd(element, CreateTypeGenerator, this);

    public IEnumerable<ITypeGenerator> GetAll() => _registry.Values;

    private static ITypeGenerator CreateTypeGenerator(ILocatedOpenApiElement<TElement> element, TypeGeneratorRegistry<TElement> registry)
    {
        if (LocatedElementEqualityComparer<TElement>.IsReferenceEqualDefault &&
            element.Element is IOpenApiReferenceable referenceable && referenceable.Reference != null)
        {
            // When making the new type generator with the factory for a reference, we must ensure
            // that we are using the referenced component path for the ILocatedOpenApiElement.

            var referencedElement = (TElement)registry._document.ResolveReference(referenceable.Reference);
            element = LocatedOpenApiElement.CreateRoot(referencedElement, referenceable.Reference.Id);
        }

        ITypeGenerator? parent = element.Parent is not null
            ? registry._mainRegistry.Get(element.Parent, registry._generatorCategory)
            : null;

        foreach (ITypeGeneratorFactory<TElement> factory in registry._factories)
        {
            ITypeGenerator? generator = factory.Create(element, parent);
            if (generator is not null)
            {
                return generator;
            }
        }

        // Fallback to the NoopTypeGeneratorFactory
        return InterlockedHelper.GetOrInitialize(ref s_noopTypeGeneratorFactory)
            .Create(element, parent);
    }
}
