using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson;

public class JsonExtensibleEnumEnricher(
    IJsonSerializationNamespace jsonNamespace)
    : IOpenApiSyntaxNodeEnricher<RecordDeclarationSyntax, OpenApiSchema>
{
    public RecordDeclarationSyntax Enrich(RecordDeclarationSyntax target, OpenApiEnrichmentContext<OpenApiSchema> context)
        => target.IsExtensibleEnumeration()
            ? EnrichEnum(target)
            : target;

    private RecordDeclarationSyntax EnrichEnum(RecordDeclarationSyntax target)
        => target
            .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(
                    NewtonsoftJsonTypes.JsonConverterAttributeName,
                    AttributeArgumentList(SingletonSeparatedList(
                        AttributeArgument(TypeOfExpression(
                            jsonNamespace.JsonExtensibleEnumConverterName(IdentifierName(target.Identifier)))))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
}
