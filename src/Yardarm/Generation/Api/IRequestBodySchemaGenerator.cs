using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Api
{
    public interface IRequestBodySchemaGenerator
    {
        void Preprocess(LocatedOpenApiElement<OpenApiRequestBody> element);

        TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiRequestBody> element);

        SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiRequestBody> element);
    }
}
