using System;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    /// <summary>
    /// Represents an <see cref="IOpenApiElement"/> element with information about the path
    /// used to reach that element in the Open API document.
    /// </summary>
    public abstract class LocatedOpenApiElement : ILocatedOpenApiElement, IEquatable<LocatedOpenApiElement>
    {
        /// <summary>
        /// The element.
        /// </summary>
        public IOpenApiElement Element { get; }

        /// <summary>
        /// Type of the element.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Key in which this element was stored on its parent.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Parent of this element.
        /// </summary>
        public ILocatedOpenApiElement? Parent { get; }

        protected LocatedOpenApiElement(IOpenApiElement element, string key)
            : this(element, key, null)
        {
        }

        protected LocatedOpenApiElement(IOpenApiElement element, string key, ILocatedOpenApiElement? parent)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Parent = parent;
        }

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

            if (!Element.Equals(other.Element) && Key != other.Key)
            {
                return false;
            }

            if (Parent is null)
            {
                if (!(other.Parent is null))
                {
                    return false;
                }
            }
            else if (other.Parent is null)
            {
                return false;
            }
            else if (!Parent.Equals(other.Parent))
            {
                return false;
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
            hashCode.Add(Parent);

            return hashCode.ToHashCode();
        }

        #endregion
    }
}
