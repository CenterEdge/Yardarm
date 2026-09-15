using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation;

internal sealed class TypeInfoAliasGenerator(ITypeGenerator underlying, ITypeGenerator? parent) : ITypeGenerator
{
    public ITypeGenerator? Parent { get; } = parent;

    public YardarmTypeInfo TypeInfo => underlying.TypeInfo;

    public QualifiedNameSyntax? GetTypeName() => underlying.GetTypeName();

    public SyntaxTree? GenerateSyntaxTree() => null;

    public IEnumerable<MemberDeclarationSyntax> Generate() => [];

    public QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        where TChild : IOpenApiElement =>
        underlying.GetChildName(child, nameKind);
}
