using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment
{
    public interface IPropertyEnricher
    {
        PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, LocatedOpenApiElement<OpenApiSchema> property);
    }
}
