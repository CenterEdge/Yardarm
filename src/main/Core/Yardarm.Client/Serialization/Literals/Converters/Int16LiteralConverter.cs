using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class Int16LiteralConverter : ValueTypeLiteralConverter<short>
{
    protected override short ReadCore(string value, string? format) =>
        short.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(short value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(short value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
