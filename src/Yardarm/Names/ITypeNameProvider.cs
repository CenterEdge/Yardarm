using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public interface ITypeNameProvider
    {
        TypeSyntax GetName(LocatedOpenApiElement element);
    }
}
