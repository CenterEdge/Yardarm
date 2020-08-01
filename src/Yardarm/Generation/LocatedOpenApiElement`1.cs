﻿using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public class LocatedOpenApiElement<T> : LocatedOpenApiElement
        where T : IOpenApiSerializable
    {
        public new T Element => (T) base.Element;

        public LocatedOpenApiElement(T element, string key)
            : base(element, key)
        {
        }

        public LocatedOpenApiElement(T element, string key, IReadOnlyList<LocatedOpenApiElement> parents)
            : base(element, key, parents)
        {
        }
    }
}
