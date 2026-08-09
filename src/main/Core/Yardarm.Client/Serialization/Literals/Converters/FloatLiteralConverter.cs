using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class FloatLiteralConverter : ValueTypeLiteralConverter<float>
{
    protected override float ReadCore(string value, string? format) =>
        float.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(float value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(float value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
