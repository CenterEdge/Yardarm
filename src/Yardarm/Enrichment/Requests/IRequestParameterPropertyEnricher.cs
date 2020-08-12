using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Requests
{
    public interface IRequestParameterPropertyEnricher : IEnricher<PropertyDeclarationSyntax, LocatedOpenApiElement<OpenApiParameter>>
    {
    }
}
