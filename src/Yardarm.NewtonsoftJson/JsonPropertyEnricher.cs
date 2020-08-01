using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;

namespace Yardarm.NewtonsoftJson
{
    public class JsonPropertyEnricher : IPropertyEnricher
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
