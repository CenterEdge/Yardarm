using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class StringLiteralConverter : ReferenceTypeLiteralConverter<string>
{
    protected override string ReadCore(string value, string? format) => value;

    protected override string WriteCore(string value, string? format) => value;

#if NET6_0_OR_GREATER

    protected override bool TryWriteCore(string value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        if (value.TryCopyTo(destination))
        {
            charsWritten = value.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

#endif
}
