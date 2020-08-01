using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Helpers;

namespace Yardarm.Generation
{
    /// <summary>
    /// Represents an <see cref="IOpenApiSerializable"/> element with information about the path
    /// used to reach that element in the Open API document.
    /// </summary>
    public class LocatedOpenApiElement
    {
        /// <summary>
        /// The element.
        /// </summary>
        public IOpenApiSerializable Element { get; }

        /// <summary>
        /// Key in which this element was stored on its parent.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// List of parents, moving from closest ancestor to towards the root.
        /// </summary>
        public IReadOnlyList<LocatedOpenApiElement> Parents { get; }

        public bool IsRoot => Parents.Count == 0;

        public LocatedOpenApiElement(IOpenApiSerializable element, string key)
            : this(element, key, Array.Empty<LocatedOpenApiElement>())
        {
        }

        public LocatedOpenApiElement(IOpenApiSerializable element, string key, IReadOnlyList<LocatedOpenApiElement> parents)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Parents = parents ?? throw new ArgumentNullException(nameof(parents));
        }

        public LocatedOpenApiElement<T> CreateChild<T>(T child, string key)
            where T : IOpenApiSerializable =>
            new LocatedOpenApiElement<T>(child, key, Parents.Push(this));

        public static LocatedOpenApiElement<T> CreateRoot<T>(T rootItem, string key)
            where T : IOpenApiSerializable =>
            new LocatedOpenApiElement<T>(rootItem, key);
    }
}
