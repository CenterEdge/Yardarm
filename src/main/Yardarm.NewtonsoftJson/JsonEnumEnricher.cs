using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonEnumEnricher : IOpenApiSyntaxNodeEnricher<EnumDeclarationSyntax, IOpenApiSchema>
    {
        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context) =>
            context.Element.HasType(JsonSchemaType.String)
                ? target
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(NewtonsoftJsonTypes.JsonConverterAttributeName).AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(NewtonsoftJsonTypes.StringEnumConverterName))))
                        .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed))
                : target;
    }
}
