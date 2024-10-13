using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.NodaTime.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime;

/// <summary>
/// Replaces date and type JSON properties with NodaTime types.
/// </summary>
public class NodaTimePropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
{
    public Type[] ExecuteBefore { get; } = [typeof(RequiredPropertyEnricher)];

    public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context)
    {
        if (context.Element.Type != "string")
        {
            // Not a string, so no change
            return target;
        }

        TypeSyntax? newType = context.Element.Format switch
        {
            "date-time" => NodaTimeTypes.OffsetDateTime,
            "date" or "full-date" => NodaTimeTypes.LocalDate,
            "partial-time" => NodaTimeTypes.LocalTime,
            "time" or "full-time" => NodaTimeTypes.OffsetTime,
            _ => null
        };

        if (newType is null)
        {
            // No change
            return target;
        }

        if (target.Type is NullableTypeSyntax)
        {
            newType = NullableType(newType);
        }

        return target.WithType(newType);
    }
}
