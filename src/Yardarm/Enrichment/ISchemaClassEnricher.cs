using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment
{
    public interface ISchemaClassEnricher
    {
        ClassDeclarationSyntax Enrich(ClassDeclarationSyntax syntax, LocatedOpenApiElement<OpenApiSchema> property);
    }
}
