using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonEnumEnricher : IOpenApiSyntaxNodeEnricher<EnumDeclarationSyntax, OpenApiSchema>
    {
        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Type == "string" && context.LocatedElement.IsJsonSchema()
                ? target
                    .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(
                            SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                            AttributeArgumentList(SingletonSeparatedList(
                                AttributeArgument(TypeOfExpression(
                                    SystemTextJsonTypes.Serialization.JsonStringEnumConverterName(IdentifierName(target.Identifier)))))))))
                        .WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                : target;
    }
}
