using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    public class NullSchemaGenerator : ITypeGenerator
    {
        public static NullSchemaGenerator Instance { get; } = new NullSchemaGenerator();

        private NullSchemaGenerator()
        {
        }

        public YardarmTypeInfo TypeInfo { get; } = new YardarmTypeInfo(
            SyntaxFactory.IdentifierName("dynamic"),
            isGenerated: false);

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
