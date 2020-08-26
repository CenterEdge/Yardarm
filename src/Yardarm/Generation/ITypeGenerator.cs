using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;

namespace Yardarm.Generation
{
    public interface ITypeGenerator : IYardarmTypeInfoProvider
    {
        SyntaxTree? GenerateSyntaxTree();

        IEnumerable<MemberDeclarationSyntax> Generate();
    }
}
