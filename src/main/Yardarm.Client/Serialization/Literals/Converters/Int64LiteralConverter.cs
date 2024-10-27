using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class Int64LiteralConverter : ValueTypeLiteralConverter<long>
{
    protected override long ReadCore(string value, string? format) =>
        long.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(long value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(long value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
