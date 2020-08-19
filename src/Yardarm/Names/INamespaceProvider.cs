using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public interface INamespaceProvider
    {
        NameSyntax GetNamespace(LocatedOpenApiElement element);
    }
}
