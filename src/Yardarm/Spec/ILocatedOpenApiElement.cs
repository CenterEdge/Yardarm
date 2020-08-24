using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public interface ILocatedOpenApiElement
    {
        IOpenApiElement Element { get; }

        string Key { get; }

        ILocatedOpenApiElement? Parent { get; }
    }
}
