using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class Int32LiteralConverter : ValueTypeLiteralConverter<int>
{
    protected override int ReadCore(string value, string? format) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(int value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(int value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
