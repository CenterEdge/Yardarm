using System;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    /// <summary>
    /// Used as a key for TGeneratorCategory on <see cref="ITypeGeneratorFactory{TElement,TGeneratorCategory}"/>.
    /// Represents the primary types generated for an element.
    /// </summary>
    public class PrimaryGeneratorCategory
    {
        private PrimaryGeneratorCategory()
        {
        }

        /// <summary>
        /// Supports DI injection of <see cref="ITypeGeneratorRegistry{TElement}"/> wrapping a
        /// <see cref="ITypeGeneratorRegistry{TElement,TGeneratorCategory}"/>.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        internal class TypeGeneratorRegistryWrapper<TElement> : ITypeGeneratorRegistry<TElement>
            where TElement : IOpenApiElement
        {
            private readonly ITypeGeneratorRegistry<TElement, PrimaryGeneratorCategory> _innerRegistry;

            public TypeGeneratorRegistryWrapper(ITypeGeneratorRegistry<TElement, PrimaryGeneratorCategory> innerRegistry)
            {
                _innerRegistry = innerRegistry;
            }

            public ITypeGenerator Get(ILocatedOpenApiElement<TElement> element) => _innerRegistry.Get(element);
        }
    }
}
