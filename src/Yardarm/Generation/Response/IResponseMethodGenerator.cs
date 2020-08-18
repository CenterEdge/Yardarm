using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public interface IResponseMethodGenerator
    {
        MethodDeclarationSyntax? Generate(LocatedOpenApiElement<OpenApiResponse> response);
    }
}
