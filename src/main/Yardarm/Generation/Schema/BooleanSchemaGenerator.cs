using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class BooleanSchemaGenerator : ITypeGenerator
    {
        private static readonly YardarmTypeInfo s_typeInfo = new(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            NameKind.Struct,
            isGenerated: false);

        public static BooleanSchemaGenerator Instance { get; } = new();

        public ITypeGenerator? Parent => null;

        public YardarmTypeInfo TypeInfo => s_typeInfo;

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() => [];

        public QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement =>
            null;
    }
}
