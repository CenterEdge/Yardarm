using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Responses
{
    public interface IResponseClassEnricher : IEnricher<ClassDeclarationSyntax, LocatedOpenApiElement<OpenApiResponse>>
    {
    }
}
