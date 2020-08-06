using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation.Schema
{
    public class BooleanSchemaGenerator : ISchemaGenerator
    {
        public static BooleanSchemaGenerator Instance { get; } = new BooleanSchemaGenerator();

        public void Preprocess()
        {
        }

        public TypeSyntax GetTypeName() =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
