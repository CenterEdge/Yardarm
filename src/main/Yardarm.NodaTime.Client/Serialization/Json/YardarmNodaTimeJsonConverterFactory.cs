using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Text;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// Extends <see cref="NodaTimeDefaultJsonConverterFactory"/> to support
/// Yardarm-specific conversions.
/// </summary>
/// <remarks>
/// In particular, uses RFC3339 style formatting for <see cref="OffsetTime"/> which always outputs the
/// minutes of the time zone offset, even if they are zero. This is required for OpenAPI 3.1 compliance.
/// </remarks>
internal sealed class YardarmNodaTimeJsonConverterFactory : JsonConverterFactory
{
    public static JsonConverter<OffsetTime> OffsetTimeConverter { get; } =
        new NodaPatternConverter<OffsetTime>(OffsetTimePattern.Rfc3339);

    private readonly NodaTimeDefaultJsonConverterFactory _innerFactory = new();

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(OffsetTime))
        {
            return true;
        }

        return _innerFactory.CanConvert(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(OffsetTime))
        {
            return OffsetTimeConverter;
        }

        return _innerFactory.CreateConverter(typeToConvert, options);
    }
}
