using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class GuidLiteralConverter : ValueTypeLiteralConverter<Guid>
{
    protected override Guid ReadCore(string value, string? format) =>
        Guid.Parse(value);

    public override string Write(Guid value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    public override bool TryWrite(Guid value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten);

#endif
}
