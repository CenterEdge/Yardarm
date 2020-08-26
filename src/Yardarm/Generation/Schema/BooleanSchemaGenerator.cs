using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    public class BooleanSchemaGenerator : ITypeGenerator
    {
        public static BooleanSchemaGenerator Instance { get; } = new BooleanSchemaGenerator();

        public YardarmTypeInfo TypeInfo { get; } = new YardarmTypeInfo(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            NameKind.Struct,
            isGenerated: false);

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
