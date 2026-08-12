using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson;

public sealed class JsonDiscriminatedUnionEnricher(
    IJsonSerializationNamespace jsonSerializationNamespace)
    : IOpenApiSyntaxNodeEnricher<StructDeclarationSyntax, OpenApiSchema>
{
    public StructDeclarationSyntax Enrich(StructDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context) =>
        target.IsExternallyDiscriminatedUnion()
            ? AddJsonConverter(target)
            : target;

    private StructDeclarationSyntax AddJsonConverter(StructDeclarationSyntax target)
    {
        TypeSyntax converterType = jsonSerializationNamespace.JsonExternallyDiscriminatedUnionConverterName(IdentifierName(target.Identifier));

        return target.AddAttributeLists(
            AttributeList(SingletonSeparatedList(Attribute(NewtonsoftJsonTypes.JsonConverterAttributeName,
                AttributeArgumentList(
                    SingletonSeparatedList(AttributeArgument(TypeOfExpression(converterType)))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
