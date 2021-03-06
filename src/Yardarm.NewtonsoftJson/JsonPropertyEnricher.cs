using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;

namespace Yardarm.NewtonsoftJson
{
    public class JsonPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            target
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                    SyntaxFactory.Attribute(NewtonsoftJsonTypes.JsonPropertyAttributeName).AddArgumentListArguments(
                        SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(context.LocatedElement.Key)))));
    }
}
