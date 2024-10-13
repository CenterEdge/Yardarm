using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.NodaTime.Helpers;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime;

/// <summary>
/// Replaces date and type JSON properties with NodaTime types and applies a JSON converter attribute.
/// </summary>
public class NodaTimePropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
{
    public Type[] ExecuteBefore { get; } = [typeof(RequiredPropertyEnricher)];

    public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context)
    {
        TypeSyntax? newType = context.Element.Format switch
        {
            "date-time" => NodaTimeTypes.OffsetDateTime,
            "date" or "full-date" => NodaTimeTypes.LocalDate,
            "partial-time" => NodaTimeTypes.LocalTime,
            "date-span" => NodaTimeTypes.Duration,
            "time" => NodaTimeTypes.OffsetTime,
            _ => null
        };

        if (newType is null)
        {
            // No change
            return target;
        }

        return target
            .WithType(newType)
            .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(
                    SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                    AttributeArgumentList(SingletonSeparatedList(
                        AttributeArgument(TypeOfExpression(NodaTimeTypes.Serialization.SystemTextJson.NodaTimeDefaultJsonConverterFactory)))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed))));
    }
}
