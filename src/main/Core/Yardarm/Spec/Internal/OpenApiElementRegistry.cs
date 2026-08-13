using Microsoft.OpenApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Yardarm.Spec.Internal
{
    /// <inheritdoc />
    internal class OpenApiElementRegistry : IOpenApiElementRegistry
    {
        private readonly ConcurrentDictionary<string, ILocatedOpenApiElement> _registry =
            new ConcurrentDictionary<string, ILocatedOpenApiElement>();

        public ILocatedOpenApiElement<T> Get<T>(string key) where T : IOpenApiElement
        {
            if (!TryGet<T>(key, out var element))
            {
                throw new KeyNotFoundException();
            }

            return element;
        }

        public bool TryGet<T>(string key, [MaybeNullWhen(false)] out ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement
        {
            if (!_registry.TryGetValue(key, out var untypedElement))
            {
                element = null;
                return false;
            }

            element = (ILocatedOpenApiElement<T>)untypedElement;
            return true;
        }

        public string Add<T>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement
        {
            ArgumentNullException.ThrowIfNull(element);

            var key = Guid.NewGuid().ToString();
            _registry.TryAdd(key, element);

            return key;
        }
    }
}
