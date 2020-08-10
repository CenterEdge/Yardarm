using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaClassEnricher : IEnricher<ClassDeclarationSyntax, LocatedOpenApiElement<OpenApiSchema>>
    {
    }
}
