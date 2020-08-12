using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Helpers
{
    internal class LocatedElementEqualityComparer<T> : IEqualityComparer<LocatedOpenApiElement<T>>
        where T : IOpenApiSerializable
    {
        public bool Equals(LocatedOpenApiElement<T>? x, LocatedOpenApiElement<T>? y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            if (x.Element is IOpenApiReferenceable referenceableX && y.Element is IOpenApiReferenceable referenceableY)
            {
                if (referenceableX.Reference != null)
                {
                    if (referenceableY.Reference == null)
                    {
                        // Can't be equal if one is a reference and the other is not
                        return false;
                    }

                    return referenceableX.Reference.ReferenceV3 == referenceableY.Reference.ReferenceV3;
                }
            }

            // Neither are references, so compare the paths

            if (!ReferenceEquals(x.Element, y.Element) || x.Key != y.Key || x.Parents.Count != y.Parents.Count)
            {
                return false;
            }

            return x.Parents.Count == 0 || Equals(x.Parents[0], y.Parents[0]);
        }

        public int GetHashCode(LocatedOpenApiElement<T> obj)
        {
            if (obj.Element is IOpenApiReferenceable referenceable && referenceable.Reference != null)
            {
                return referenceable.Reference.ReferenceV3.GetHashCode();
            }
            else
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Element);
                hashCode.Add(obj.Key);

                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < obj.Parents.Count; i++)
                {
                    hashCode.Add(obj.Parents[i].Key);
                }

                return hashCode.ToHashCode();
            }
        }
    }
}
