using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson;

public sealed class JsonDiscriminatedUnionEnricher(
    IJsonSerializationNamespace jsonSerializationNamespace)
    : IOpenApiSyntaxNodeEnricher<StructDeclarationSyntax, IOpenApiSchema>
{
    public StructDeclarationSyntax Enrich(StructDeclarationSyntax target,
        OpenApiEnrichmentContext<IOpenApiSchema> context) =>
        target.IsExternallyDiscriminatedUnion()
            ? AddJsonConverter(target)
            : target;

    private StructDeclarationSyntax AddJsonConverter(StructDeclarationSyntax target)
    {
        TypeSyntax converterType = jsonSerializationNamespace.JsonExternallyDiscriminatedUnionConverterName(IdentifierName(target.Identifier));

        return target.AddAttributeLists(
            AttributeList(SingletonSeparatedList(Attribute(SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                AttributeArgumentList(
                    SingletonSeparatedList(AttributeArgument(TypeOfExpression(converterType)))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
