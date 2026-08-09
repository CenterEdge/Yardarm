using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Yardarm.Spec
{
    public class TagComparer : IEqualityComparer<ILocatedOpenApiElement<OpenApiTag>>
    {
        public static TagComparer Instance { get; } = new();

        private TagComparer()
        {
        }

        public bool Equals(ILocatedOpenApiElement<OpenApiTag>? x, ILocatedOpenApiElement<OpenApiTag>? y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            return x.Element.Name == y.Element.Name;
        }

        public int GetHashCode(ILocatedOpenApiElement<OpenApiTag> obj) => obj.Element.Name.GetHashCode();
    }
}
