using System;
using System.Globalization;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class DoubleLiteralConverter : ValueTypeLiteralConverter<double>
{
    protected override double ReadCore(string value, string? format) =>
        double.Parse(value, CultureInfo.InvariantCulture);

    public override string Write(double value, string? format) =>
        value.ToString(CultureInfo.InvariantCulture);

#if NET6_0_OR_GREATER

    public override bool TryWrite(double value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);

#endif
}
