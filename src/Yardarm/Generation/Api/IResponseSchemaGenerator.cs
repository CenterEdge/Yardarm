using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public interface IResponseSchemaGenerator
    {
        void Preprocess(LocatedOpenApiElement<OpenApiResponse> element);

        TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiResponse> element);

        SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiResponse> element);
    }
}
