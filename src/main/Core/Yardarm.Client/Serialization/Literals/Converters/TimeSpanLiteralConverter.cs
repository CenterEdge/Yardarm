using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class TimeSpanLiteralConverter : ValueTypeLiteralConverter<TimeSpan>
{
    protected override TimeSpan ReadCore(string value, string? format) =>
        format switch
        {
            "partial-time" or "date-span" => TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture),
            _ => TimeSpan.Parse(value, CultureInfo.InvariantCulture)
        };

    public override string Write(TimeSpan value, string? format) =>
        value.ToString("c");

#if NET6_0_OR_GREATER

    public override bool TryWrite(TimeSpan value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, format: "c");

#endif
}
