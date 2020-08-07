using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface INamespaceProvider
    {
        NameSyntax GetRootNamespace();
        NameSyntax GetNamespace(LocatedOpenApiElement element);
    }
}
