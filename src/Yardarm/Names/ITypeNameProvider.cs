using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    public interface ITypeNameProvider
    {
        TypeSyntax TypeName { get; }
    }
}
