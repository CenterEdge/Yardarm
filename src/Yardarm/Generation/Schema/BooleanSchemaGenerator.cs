using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class BooleanSchemaGenerator : ISchemaGenerator
    {
        public TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiSchema> schemaElement) =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element) => null;

        public IEnumerable<MemberDeclarationSyntax> Generate(LocatedOpenApiElement<OpenApiSchema> element) =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
