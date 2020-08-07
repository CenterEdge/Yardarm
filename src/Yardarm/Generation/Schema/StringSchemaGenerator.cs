using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation.Schema
{
    public class StringSchemaGenerator : ITypeGenerator
    {
        public static StringSchemaGenerator Instance { get; } = new StringSchemaGenerator();

        public void Preprocess()
        {
        }

        public TypeSyntax GetTypeName() =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
