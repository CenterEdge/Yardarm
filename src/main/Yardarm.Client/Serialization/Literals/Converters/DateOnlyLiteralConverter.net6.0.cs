using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class DateOnlyLiteralConverter : ValueTypeLiteralConverter<DateOnly>
{
    protected override DateOnly ReadCore(string value, string? format) =>
        DateOnly.ParseExact(value, "o");

    public override string Write(DateOnly value, string? format) =>
        value.ToString("o");

    public override bool TryWrite(DateOnly value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, "o");
}
