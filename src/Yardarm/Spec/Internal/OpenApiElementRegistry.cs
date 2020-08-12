using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec.Internal
{
    /// <inheritdoc />
    internal class OpenApiElementRegistry : IOpenApiElementRegistry
    {
        private readonly ConcurrentDictionary<string, LocatedOpenApiElement> _registry =
            new ConcurrentDictionary<string, LocatedOpenApiElement>();

        public LocatedOpenApiElement<T> Get<T>(string key) where T : IOpenApiSerializable
        {
            if (!TryGet<T>(key, out var element))
            {
                throw new KeyNotFoundException();
            }

            return element;
        }

        public bool TryGet<T>(string key, [MaybeNullWhen(false)] out LocatedOpenApiElement<T> element)
            where T : IOpenApiSerializable
        {
            if (!_registry.TryGetValue(key, out var untypedElement))
            {
                element = null;
                return false;
            }

            element = (LocatedOpenApiElement<T>)untypedElement;
            return true;
        }

        public string Add<T>(LocatedOpenApiElement<T> element)
            where T : IOpenApiSerializable
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var key = Guid.NewGuid().ToString();
            _registry.TryAdd(key, element);

            return key;
        }
    }
}
