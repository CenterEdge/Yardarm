using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class UInt64LiteralConverter : ValueTypeLiteralConverter<ulong>
{
    // CultureInfo.InvariantCulture is not needed for unsigned integers

    protected override ulong ReadCore(string value, string? format) =>
        ulong.Parse(value);

    public override string Write(ulong value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    public override bool TryWrite(ulong value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten);

#endif
}
