using NodaTime;
using NodaTime.Text;
using NodaTime.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals.Converters;

internal static class NodaLiteralConverters
{
    /// <summary>
    /// Converter for local dates, using the ISO-8601 date pattern.
    /// </summary>
    public static LiteralConverter<LocalDate> LocalDateConverter => field ??=
        new NodaPatternLiteralConverter<LocalDate>(
            LocalDatePattern.Iso, CreateIsoValidator<LocalDate>(x => x.Calendar));

    /// <summary>
    /// Converter for local date/times, using the ISO-8601 date pattern without a time zone identifier.
    /// </summary>
    public static LiteralConverter<LocalDateTime> LocalDateTimeConverter => field ??=
        new NodaPatternLiteralConverter<LocalDateTime>(
            LocalDateTimePattern.ExtendedIso, CreateIsoValidator<LocalDateTime>(x => x.Calendar));

    /// <summary>
    /// Converter for local times, using the ISO-8601 time pattern, extended as required to accommodate nanoseconds.
    /// </summary>
    public static LiteralConverter<LocalTime> LocalTimeConverter => field ??=
        new NodaPatternLiteralConverter<LocalTime>(LocalTimePattern.ExtendedIso);

    /// <summary>
    /// Converter for offset date/times.
    /// </summary>
    public static LiteralConverter<OffsetDateTime> OffsetDateTimeConverter => field ??=
        new NodaPatternLiteralConverter<OffsetDateTime>(
            OffsetDateTimePattern.Rfc3339, CreateIsoValidator<OffsetDateTime>(x => x.Calendar));

    /// <summary>
    /// Converter for offset times.
    /// </summary>
    public static LiteralConverter<OffsetTime> OffsetTimeConverter => field ??=
        new NodaPatternLiteralConverter<OffsetTime>(OffsetTimePattern.Rfc3339);

    /// <summary>
    /// Converter for periods.
    /// </summary>
    public static LiteralConverter<Period?> PeriodConverter => field ??=
        new NodaRefTypePatternLiteralConverter<Period>(PeriodPattern.Roundtrip);

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
