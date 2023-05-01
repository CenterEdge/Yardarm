using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Spec;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonOptionalPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        private readonly IOpenApiElementRegistry _elementRegistry;

        public JsonOptionalPropertyEnricher(IOpenApiElementRegistry elementRegistry)
        {
            _elementRegistry = elementRegistry ?? throw new ArgumentNullException(nameof(elementRegistry));
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (!context.LocatedElement.IsJsonSchema())
            {
                // Don't enrich non-JSON schemas
                return syntax;
            }

            if (syntax.Parent?.GetElementAnnotation<OpenApiSchema>(_elementRegistry) is null)
            {
                // We don't need to apply this to properties of request classes, only schemas
                return syntax;
            }

            bool isRequired =
                context.LocatedElement.Parent is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
                parentSchema.Element.Required.Contains(context.LocatedElement.Key);

            bool isNullable = context.LocatedElement.Element.Nullable;

            // We prefer not to send null values if the property is not required.
            // However, for nullable properties, prefer to send the null explicitly.
            // This is a compromise due to .NET not supporting a concept of null vs missing.
            return !isRequired && !isNullable
                ? AddJsonIgnoreAttribute(syntax)
                : syntax;
        }

        private PropertyDeclarationSyntax AddJsonIgnoreAttribute(PropertyDeclarationSyntax syntax) =>
            syntax
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(SystemTextJsonTypes.Serialization.JsonIgnoreAttributeName,
                        AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                            NameEquals(IdentifierName("Condition")),
                            null,
                            SystemTextJsonTypes.Serialization.JsonIgnoreCondition.WhenWritingNull))))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
