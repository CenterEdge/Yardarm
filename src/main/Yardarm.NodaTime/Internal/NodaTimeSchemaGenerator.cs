using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.NodaTime.Helpers;
using Yardarm.Spec;

namespace Yardarm.NodaTime.Internal;

internal sealed class NodaTimeSchemaGenerator(
    ILocatedOpenApiElement<OpenApiSchema> schemaElement,
    GenerationContext context,
    ITypeGenerator? parent)
    : TypeGeneratorBase<OpenApiSchema>(schemaElement, context, parent)
{
    public static FrozenSet<string> SupportedFormats { get; } = new[]
    {
        "date-time",
        "date-time-local",
        "date",
        "full-date",
        "partial-time",
        "time",
        "full-time"
    }.ToFrozenSet();

    private static YardarmTypeInfo? s_localDate;
    private static YardarmTypeInfo LocalDate => s_localDate ??= new YardarmTypeInfo(
        NodaTimeTypes.LocalDate, isGenerated: false);

    private static YardarmTypeInfo? s_localDateTime;
    private static YardarmTypeInfo LocalDateTime => s_localDateTime ??= new YardarmTypeInfo(
        NodaTimeTypes.LocalDateTime, isGenerated: false);

    private static YardarmTypeInfo? s_localTime;
    private static YardarmTypeInfo LocalTime => s_localTime ??= new YardarmTypeInfo(
        NodaTimeTypes.LocalTime, isGenerated: false);

    private static YardarmTypeInfo? s_offsetDateTime;
    private static YardarmTypeInfo OffsetDateTime => s_offsetDateTime ??= new YardarmTypeInfo(
        NodaTimeTypes.OffsetDateTime, isGenerated: false);

    private static YardarmTypeInfo? s_offsetTime;
    private static YardarmTypeInfo OffsetTime => s_offsetTime ??= new YardarmTypeInfo(
        NodaTimeTypes.OffsetTime, isGenerated: false);

    protected override YardarmTypeInfo GetTypeInfo()
    {
        YardarmTypeInfo? typeInfo = Element.Element.Format switch
        {
            "date-time" => OffsetDateTime,
            "date-time-local" => LocalDateTime,
            "date" or "full-date" => LocalDate,
            "partial-time" => LocalTime,
            "time" or "full-time" => OffsetTime,
            _ => null
        };

        if (typeInfo is null)
        {
            throw new InvalidOperationException("Unsupported format.");
        }

        return typeInfo;
    }

    public override SyntaxTree? GenerateSyntaxTree() => null;

    public override IEnumerable<MemberDeclarationSyntax> Generate() => [];
}
