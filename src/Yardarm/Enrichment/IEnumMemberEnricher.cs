using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Enrichment
{
    public interface IEnumMemberEnricher : IEnricher<EnumMemberDeclarationSyntax, (LocatedOpenApiElement<OpenApiSchema> Element, IOpenApiAny Member)>
    {
    }
}
