using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment
{
    public interface IEnumEnricher : IEnricher<EnumDeclarationSyntax, LocatedOpenApiElement<OpenApiSchema>>
    {
    }
}
