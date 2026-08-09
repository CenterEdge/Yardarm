using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class BooleanLiteralConverter : ValueTypeLiteralConverter<bool>
{
    protected override bool ReadCore(string value, string? format) =>
        bool.Parse(value);

    public override string Write(bool value, string? format) => value ? "true" : "false";

#if NET6_0_OR_GREATER

    public override bool TryWrite(bool value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        var boolString = value ? "true" : "false";
        if (boolString.TryCopyTo(destination))
        {
            charsWritten = boolString.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

#endif
}
