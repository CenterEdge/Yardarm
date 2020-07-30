using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface ITypeNameGenerator
    {
        TypeSyntax GetName(LocatedOpenApiElement element);
    }
}
