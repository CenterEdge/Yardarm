using System;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase<T> : TypeGeneratorBase
        where T : IOpenApiSerializable
    {
        protected LocatedOpenApiElement<T> Element { get; }

        protected TypeGeneratorBase(LocatedOpenApiElement<T> element, GenerationContext context)
            : base(context)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }
    }
}
