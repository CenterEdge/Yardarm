using NodaTime;
using NodaTime.Text;
using NodaTime.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals.Converters;

internal static class NodaLiteralConverters
{
    private static NodaPatternLiteralConverter<LocalDate>? s_localDateConverter;
    private static NodaPatternLiteralConverter<LocalTime>? s_localTimeConverter;
    private static NodaPatternLiteralConverter<OffsetDateTime>? s_offsetDateTimeConverter;
    private static NodaPatternLiteralConverter<OffsetTime>? s_offsetTimeConverter;

    /// <summary>
    /// Converter for local dates, using the ISO-8601 date pattern.
    /// </summary>
    public static LiteralConverter<LocalDate> LocalDateConverter => s_localDateConverter ??=
        new NodaPatternLiteralConverter<LocalDate>(
            LocalDatePattern.Iso, CreateIsoValidator<LocalDate>(x => x.Calendar));

    /// <summary>
    /// Converter for local times, using the ISO-8601 time pattern, extended as required to accommodate nanoseconds.
    /// </summary>
    public static LiteralConverter<LocalTime> LocalTimeConverter => s_localTimeConverter ??=
        new NodaPatternLiteralConverter<LocalTime>(LocalTimePattern.ExtendedIso);

    /// <summary>
    /// Converter for offset date/times.
    /// </summary>
    public static LiteralConverter<OffsetDateTime> OffsetDateTimeConverter => s_offsetDateTimeConverter ??=
        new NodaPatternLiteralConverter<OffsetDateTime>(
            OffsetDateTimePattern.Rfc3339, CreateIsoValidator<OffsetDateTime>(x => x.Calendar));

    /// <summary>
    /// Converter for offset times.
    /// </summary>
    public static LiteralConverter<OffsetTime> OffsetTimeConverter => s_offsetTimeConverter ??=
        new NodaPatternLiteralConverter<OffsetTime>(OffsetTimePattern.Rfc3339);

    private static Action<T> CreateIsoValidator<T>(Func<T, CalendarSystem> calendarProjection) => value =>
    {
        CalendarSystem calendar = calendarProjection(value);

        if (calendar != CalendarSystem.Iso)
        {
            ThrowInvalidNodaDataException($"Values of type {typeof(T).Name} must (currently) use the ISO calendar in order to be serialized.");
        }
    };

    [DoesNotReturn]
    private static void ThrowInvalidNodaDataException(string message)
    {
        throw new InvalidNodaDataException(message);
    }
}
