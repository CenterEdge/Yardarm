using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class SchemaEqualityComparer : IEqualityComparer<LocatedOpenApiElement<OpenApiSchema>>
    {
        public bool Equals(LocatedOpenApiElement<OpenApiSchema>? x, LocatedOpenApiElement<OpenApiSchema>? y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            if (x.Element.Reference != null)
            {
                if (y.Element.Reference == null)
                {
                    // Can't be equal if one is a reference and the other is not
                    return false;
                }

                return x.Element.Reference.ReferenceV3 == y.Element.Reference.ReferenceV3;
            }

            // Neither are references, so compare the paths

            if (!ReferenceEquals(x.Element, y.Element) || x.Key != y.Key || x.Parents.Count != y.Parents.Count)
            {
                return false;
            }

            return x.Parents.Count == 0 || Equals(x.Parents[0], y.Parents[0]);
        }

        public int GetHashCode(LocatedOpenApiElement<OpenApiSchema> obj)
        {
            if (obj.Element.Reference != null)
            {
                return obj.Element.Reference.ReferenceV3.GetHashCode();
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
