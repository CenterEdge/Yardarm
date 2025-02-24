using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation;

internal class NoopTypeGenerator<T>(ITypeGenerator? parent) : ITypeGenerator
    where T : IOpenApiElement
{
    public ITypeGenerator? Parent { get; } = parent;

    public YardarmTypeInfo TypeInfo { get; } = new YardarmTypeInfo(
        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
        NameKind.Struct,
        isGenerated: false);

    public SyntaxTree? GenerateSyntaxTree() => null;

    public IEnumerable<MemberDeclarationSyntax> Generate() => [];

    public QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        where TChild : IOpenApiElement =>
        Parent?.GetChildName(child, nameKind);
}
