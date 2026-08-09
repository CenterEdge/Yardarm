using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson;

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
                    SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                    AttributeArgumentList(SingletonSeparatedList(
                        AttributeArgument(TypeOfExpression(
                            jsonNamespace.JsonExtensibleEnumConverterName(IdentifierName(target.Identifier)))))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
}
