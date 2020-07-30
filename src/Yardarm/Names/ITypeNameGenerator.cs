using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface ITypeNameGenerator
    {
        QualifiedNameSyntax GetName(IOpenApiReferenceable component, IEnumerable<OpenApiPathElement> parent, string key);
    }
}
