using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGenerator
    {
        SyntaxTree? Generate(OpenApiSchema schema, string key);

        MemberDeclarationSyntax? Generate(OpenApiSchema schema, OpenApiPathElement[] parents, string key);
    }
}
