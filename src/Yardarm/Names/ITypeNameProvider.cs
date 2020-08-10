using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface ITypeNameProvider
    {
        TypeSyntax GetName(LocatedOpenApiElement element);
    }
}
