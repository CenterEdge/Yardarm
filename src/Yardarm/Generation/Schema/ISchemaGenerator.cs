using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGenerator
    {
        TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiSchema> schemaElement);

        SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element);

        MemberDeclarationSyntax? Generate(LocatedOpenApiElement<OpenApiSchema> element);
    }
}
