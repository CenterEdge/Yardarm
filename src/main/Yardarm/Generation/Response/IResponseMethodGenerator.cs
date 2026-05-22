using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public interface IResponseMethodGenerator
    {
        IEnumerable<BaseMethodDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiResponse> response, string className);
    }
}
