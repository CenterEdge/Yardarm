using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public class LocatedOpenApiElement<T> : LocatedOpenApiElement, ILocatedOpenApiElement<T>
        where T : IOpenApiElement
    {
        public new T Element => (T) base.Element;

        public LocatedOpenApiElement(T element, string key)
            : base(element, key)
        {
        }

        public LocatedOpenApiElement(T element, string key, IReadOnlyList<ILocatedOpenApiElement> parents)
            : base(element, key, parents)
        {
        }
    }
}
