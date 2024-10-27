using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class UInt16LiteralConverter : ValueTypeLiteralConverter<ushort>
{
    // CultureInfo.InvariantCulture is not needed for unsigned integers

    protected override ushort ReadCore(string value, string? format) =>
        ushort.Parse(value);

    public override string Write(ushort value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    public override bool TryWrite(ushort value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten);

#endif
}
