using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public interface ITypeGenerator : IYardarmTypeInfoProvider
    {
        ITypeGenerator? Parent { get; }

        SyntaxTree? GenerateSyntaxTree();

        IEnumerable<MemberDeclarationSyntax> Generate();

        QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement;
    }
}
