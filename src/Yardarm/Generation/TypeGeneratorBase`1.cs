using System;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase<T> : TypeGeneratorBase
        where T : IOpenApiElement
    {
        protected ILocatedOpenApiElement<T> Element { get; }

        protected TypeGeneratorBase(ILocatedOpenApiElement<T> element, GenerationContext context)
            : base(context)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }
    }
}
