using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaClassEnricher : IEnricher<ClassDeclarationSyntax, LocatedOpenApiElement<OpenApiSchema>>
    {
    }
}
