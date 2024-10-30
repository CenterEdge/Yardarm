using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class DateTimeOffsetLiteralConverter : ValueTypeLiteralConverter<DateTimeOffset>
{
    protected override DateTimeOffset ReadCore(string value, string? format) =>
        format switch
        {
            "date" or "full-date" => DateTimeOffset.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture)
        };

    public override string Write(DateTimeOffset value, string? format) =>
        format switch
        {
            "date" or "full-date" => value.ToString("yyyy-MM-dd"),
            _ => value.ToString("O")
        };

#if NET6_0_OR_GREATER

    public override bool TryWrite(DateTimeOffset value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        format switch
        {
            "date" or "full-date" => value.TryFormat(destination, out charsWritten, format: "yyyy-MM-dd"),
            _ => value.TryFormat(destination, out charsWritten, format: "O")
        };

#endif
}
