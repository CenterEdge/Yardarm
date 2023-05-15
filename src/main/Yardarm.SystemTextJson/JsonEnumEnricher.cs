using System;
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
        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public JsonEnumEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            _jsonSerializationNamespace = jsonSerializationNamespace ?? throw new ArgumentNullException(nameof(jsonSerializationNamespace));
        }

        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Type == "string" && context.LocatedElement.IsJsonSchema()
                ? target
                    .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(
                            SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                            AttributeArgumentList(SingletonSeparatedList(
                                AttributeArgument(TypeOfExpression(
                                    _jsonSerializationNamespace.JsonStringEnumConverter(IdentifierName(target.Identifier)))))))))
                        .WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                : target;
    }
}
