using System;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public sealed class LocatedOpenApiElement<T> : LocatedOpenApiElement, ILocatedOpenApiElement<T>
        where T : IOpenApiElement
    {
        public new T Element => (T) base.Element;

        public override Type ElementType => typeof(T);

        public LocatedOpenApiElement(T element, string key)
            : base(element, key)
        {
        }

        public LocatedOpenApiElement(T element, string key, ILocatedOpenApiElement? parent)
            : base(element, key, parent)
        {
        }

        public override string ToString() =>
            Parent != null
                ? $"{Parent}/{Key}:{typeof(T).Name}"
                : $"{Key}:{typeof(T).Name}";
    }
}
