using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public interface IElementTypeNameProvider
    {
        TypeSyntax GetName(ILocatedOpenApiElement element);
    }
}
