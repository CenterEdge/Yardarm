using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public interface IResponseMethodGenerator
    {
        MethodDeclarationSyntax? Generate(ILocatedOpenApiElement<OpenApiResponse> response);
    }
}
