using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Responses
{
    public interface IResponseHeaderPropertyEnricher : IEnricher<PropertyDeclarationSyntax, LocatedOpenApiElement<OpenApiHeader>>
    {
    }
}
