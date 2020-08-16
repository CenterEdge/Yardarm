using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public interface IBuildUriMethodGenerator
    {
        MethodDeclarationSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation);
    }
}
