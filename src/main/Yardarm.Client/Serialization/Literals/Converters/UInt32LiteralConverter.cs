using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class UInt32LiteralConverter : ValueTypeLiteralConverter<uint>
{
    // CultureInfo.InvariantCulture is not needed for unsigned integers

    protected override uint ReadCore(string value, string? format) =>
        uint.Parse(value);

    public override string Write(uint value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    public override bool TryWrite(uint value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten);

#endif
}
