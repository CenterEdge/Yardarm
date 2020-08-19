using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Spec
{
    /// <summary>
    /// Represents an <see cref="IOpenApiElement"/> element with information about the path
    /// used to reach that element in the Open API document.
    /// </summary>
    public abstract class LocatedOpenApiElement : IEquatable<LocatedOpenApiElement>
    {
        /// <summary>
        /// The element.
        /// </summary>
        public IOpenApiElement Element { get; }

        /// <summary>
        /// Key in which this element was stored on its parent.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// List of parents, moving from closest ancestor to towards the root.
        /// </summary>
        public IReadOnlyList<LocatedOpenApiElement> Parents { get; }

        public bool IsRoot => Parents.Count == 0;

        public LocatedOpenApiElement(IOpenApiElement element, string key)
            : this(element, key, Array.Empty<LocatedOpenApiElement>())
        {
        }

        public LocatedOpenApiElement(IOpenApiElement element, string key, IReadOnlyList<LocatedOpenApiElement> parents)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Parents = parents ?? throw new ArgumentNullException(nameof(parents));
        }

        public LocatedOpenApiElement<T> CreateChild<T>(T child, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(child, key, Parents.Push(this));

        public static LocatedOpenApiElement<T> CreateRoot<T>(T rootItem, string key)
            where T : IOpenApiElement =>
            new LocatedOpenApiElement<T>(rootItem, key);

        #region Equality

        public bool Equals(LocatedOpenApiElement? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!Element.Equals(other.Element) && Key != other.Key || Parents.Count != other.Parents.Count)
            {
                return false;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < Parents.Count; i++)
            {
                if (!Parents[i].Equals(other.Parents[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((LocatedOpenApiElement) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Element);
            hashCode.Add(Key);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < Parents.Count; i++)
            {
                hashCode.Add(Parents[i]);
            }

            return hashCode.ToHashCode();
        }

        #endregion
    }
}
