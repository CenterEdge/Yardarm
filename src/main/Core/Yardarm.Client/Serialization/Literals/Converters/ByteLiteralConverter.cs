using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class ByteLiteralConverter : ValueTypeLiteralConverter<byte>
{
    // CultureInfo.InvariantCulture is not needed for unsigned integers

    protected override byte ReadCore(string value, string? format) =>
        byte.Parse(value);

    public override string Write(byte value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    public override bool TryWrite(byte value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten);

#endif
}
