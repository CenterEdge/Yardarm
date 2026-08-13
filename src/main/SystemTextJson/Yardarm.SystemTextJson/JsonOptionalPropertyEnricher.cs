using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Spec;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson;

public class JsonOptionalPropertyEnricher(
    IOpenApiElementRegistry elementRegistry,
    IOptions<JsonOptions> jsonOptions)
    : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, IOpenApiSchema>
{
    public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<IOpenApiSchema> context)
    {
        if (!context.LocatedElement.IsJsonSchema)
        {
            // Don't enrich non-JSON schemas
            return syntax;
        }

        if (syntax.Parent?.GetElementAnnotation<IOpenApiSchema>(elementRegistry) is null)
        {
            // We don't need to apply this to properties of request classes, only schemas
            return syntax;
        }

        bool isRequired =
            context.LocatedElement.Parent is LocatedOpenApiElement<IOpenApiSchema> parentSchema &&
            parentSchema.Element.Required?.Contains(context.LocatedElement.Key) == true;

        bool isNullable = context.LocatedElement.Element.Nullable;

        // For required properties, enforce the requirement on deserialization, unless the feature is disabled.
        // We prefer not to send null values if the property is not required.
        // However, for nullable properties, prefer to send the null explicitly.
        // This is a compromise due to .NET not supporting a concept of null vs missing.
        return isRequired
            ? jsonOptions.Value.EffectiveEnforceRequiredProperties
                ? AddJsonRequiredAttribute(syntax)
                : syntax
            : !isNullable
                ? AddJsonIgnoreAttribute(syntax)
                : syntax;
    }

    private static PropertyDeclarationSyntax AddJsonRequiredAttribute(PropertyDeclarationSyntax syntax) =>
    syntax
        .AddAttributeLists(AttributeList(SingletonSeparatedList(
            Attribute(
                SystemTextJsonTypes.Serialization.JsonRequiredAttributeName,
                argumentList: default)))
            .WithTrailingTrivia(ElasticCarriageReturnLineFeed));

    private static PropertyDeclarationSyntax AddJsonIgnoreAttribute(PropertyDeclarationSyntax syntax) =>
        syntax
            .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(SystemTextJsonTypes.Serialization.JsonIgnoreAttributeName,
                    AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                        NameEquals(IdentifierName("Condition")),
                        null,
                        SystemTextJsonTypes.Serialization.JsonIgnoreCondition.WhenWritingNull))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
}
