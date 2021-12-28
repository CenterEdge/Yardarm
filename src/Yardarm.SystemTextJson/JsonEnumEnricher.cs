using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.SystemTextJson.Helpers;

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
            context.Element.Type == "string"
                ? target
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(SystemTextJsonTypes.JsonConverterAttributeName).AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(
                                _jsonSerializationNamespace.JsonStringEnumConverter(SyntaxFactory.IdentifierName(target.Identifier)))))))
                : target;
    }
}
