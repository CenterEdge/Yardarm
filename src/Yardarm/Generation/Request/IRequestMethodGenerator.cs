using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public interface IRequestMethodGenerator
    {
        MethodDeclarationSyntax GenerateHeader(ILocatedOpenApiElement<OpenApiOperation> operation);
        MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation);
    }
}
