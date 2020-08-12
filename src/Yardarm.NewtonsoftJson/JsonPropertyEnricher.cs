using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonPropertyEnricher : ISchemaPropertyEnricher
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
