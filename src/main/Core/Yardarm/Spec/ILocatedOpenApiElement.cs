using System;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public interface ILocatedOpenApiElement
    {
        IOpenApiElement Element { get; }

        Type ElementType { get; }

        string Key { get; }

        ILocatedOpenApiElement? Parent { get; }
    }
}
