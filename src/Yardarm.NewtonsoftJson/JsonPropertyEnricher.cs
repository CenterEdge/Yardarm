using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiSchema> context) =>
            target
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                    SyntaxFactory.Attribute(JsonHelpers.JsonPropertyAttributeName()).AddArgumentListArguments(
                        SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(context.Key)))));
    }
}
