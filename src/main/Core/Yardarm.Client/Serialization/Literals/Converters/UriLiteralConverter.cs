using System;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class UriLiteralConverter : ReferenceTypeLiteralConverter<Uri>
{
    protected override Uri ReadCore(string value, string? format)
    {
        if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
        {
            return uri;
        }

        ThrowHelper.ThrowFormatException("Invalid URI format.");
        return null!;
    }

    protected override string WriteCore(Uri value, string? format) =>
        value.ToString();

#if NET6_0_OR_GREATER

    protected override bool TryWriteCore(Uri value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
#if NET8_0_OR_GREATER
        return value.TryFormat(destination, out charsWritten);
#else
        var str = value.ToString();

        if (str.TryCopyTo(destination))
        {
            charsWritten = str.Length;
            return true;
        }

        charsWritten = 0;
        return false;
#endif
    }

#endif
}
