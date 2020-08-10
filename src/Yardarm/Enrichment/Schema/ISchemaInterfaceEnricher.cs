using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaInterfaceEnricher : IEnricher<InterfaceDeclarationSyntax, LocatedOpenApiElement<OpenApiSchema>>
    {
    }
}
