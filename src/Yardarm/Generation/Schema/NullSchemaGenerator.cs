using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class NullSchemaGenerator : ISchemaGenerator
    {
        public static NullSchemaGenerator Instance { get; } = new NullSchemaGenerator();

        private NullSchemaGenerator()
        {
        }

        public TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiSchema> schemaElement) =>
            SyntaxFactory.IdentifierName("dynamic");

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element) => null;

        public MemberDeclarationSyntax? Generate(LocatedOpenApiElement<OpenApiSchema> element) => null;
    }
}
