using System;
using System.Collections.Generic;
using Microsoft.OpenApi;

namespace Yardarm.Spec
{
    internal class LocatedElementEqualityComparer<T> : IEqualityComparer<ILocatedOpenApiElement<T>>
        where T : IOpenApiElement
    {
        /// <summary>
        /// If true, indicates that two elements with the same reference are considered equal.
        /// </summary>
        public bool IsReferenceEqual { get; }

        public LocatedElementEqualityComparer() : this(IsReferenceEqualDefault)
        {
        }

        public LocatedElementEqualityComparer(bool isReferenceEqual)
        {
            IsReferenceEqual = isReferenceEqual;
        }

        public bool Equals(ILocatedOpenApiElement<T>? x, ILocatedOpenApiElement<T>? y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            if (IsReferenceEqual &&
                x.Element is IOpenApiReferenceHolder &&
                y.Element is IOpenApiReferenceHolder)
            {
                var refX = x.Element.GetReferenceV3();
                var refY = y.Element.GetReferenceV3();

                if (refX != null)
                {
                    if (refY == null)
                    {
                        // Can't be equal if one is a reference and the other is not
                        return false;
                    }

                    return refX == refY;
                }
            }

            // Neither are references, so compare the paths

            if (!ReferenceEquals(x.Element, y.Element) || x.Key != y.Key || (x.Parent is null) != (y.Parent is null))
            {
                return false;
            }

            return x.Parent is null || Equals(x.Parent, y.Parent);
        }

        public int GetHashCode(ILocatedOpenApiElement<T> obj)
        {
            var refV3 = obj.Element.GetReferenceV3();
            if (refV3 != null)
            {
                return refV3.GetHashCode();
            }
            else
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Element);
                hashCode.Add(obj.Key);
                hashCode.Add(obj.Parent);

                return hashCode.ToHashCode();
            }
        }

        // For OpenApiResponse and OpenApiRequestBody, treat the element in the components section
        // as unequal to an element referencing it in an operation, allowing us to define a separate
        // class for each case.
        public static bool IsReferenceEqualDefault { get; } =
            typeof(T) != typeof(OpenApiResponse) && typeof(T) != typeof(OpenApiRequestBody);
    }
}
