using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    public interface IKnownNamespace
    {
        NameSyntax Name { get; }
    }
}
