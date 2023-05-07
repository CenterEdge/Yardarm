using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class NullSchemaGenerator : ITypeGenerator
    {
        public static NullSchemaGenerator Instance { get; } = new NullSchemaGenerator();

        public ITypeGenerator? Parent => null;

        private NullSchemaGenerator()
        {
        }

        public YardarmTypeInfo TypeInfo { get; } = new(
            NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))),
            isGenerated: false);

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();

        public QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement =>
            null;
    }
}
