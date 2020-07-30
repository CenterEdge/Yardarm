using Microsoft.CodeAnalysis;
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

        public SyntaxTree? Generate(OpenApiSchema schema, string key) => null;

        public MemberDeclarationSyntax? Generate(OpenApiSchema schema, OpenApiPathElement[] parents, string key) => null;
    }
}
