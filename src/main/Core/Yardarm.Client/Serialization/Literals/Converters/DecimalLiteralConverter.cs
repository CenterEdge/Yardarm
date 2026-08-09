using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class DecimalLiteralConverter : ValueTypeLiteralConverter<decimal>
{
    protected override decimal ReadCore(string value, string? format) =>
        decimal.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(decimal value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(decimal value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
