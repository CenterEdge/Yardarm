using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class SByteLiteralConverter : ValueTypeLiteralConverter<sbyte>
{
    protected override sbyte ReadCore(string value, string? format) =>
        sbyte.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(sbyte value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(sbyte value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
