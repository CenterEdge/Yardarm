using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals.Converters;

/// <summary>
/// Converter for nullable value types wrapping a non-nullable literal converter.
/// </summary>
/// <typeparam name="T">Type to convert.</typeparam>
/// <param name="innerConverter">Converter to use when value is not null.</param>
internal sealed class NullableLiteralConverter<T>(LiteralConverter<T> innerConverter) : LiteralConverter<T?> where T : struct
{
    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(value))]
    public override T? Read(string? value, string? format)
    {
        if (value is null)
        {
            return null;
        }

        return innerConverter.Read(value, format);
    }

    /// <inheritdoc />
    public override string Write(T? value, string? format)
    {
        if (value is null)
        {
            return "";
        }

        return innerConverter.Write(value.GetValueOrDefault(), format);
    }

#if NET6_0_OR_GREATER

    /// <inheritdoc />
    public override bool TryWrite(T? value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        if (value is null)
        {
            charsWritten = 0;
            return true;
        }

        return innerConverter.TryWrite(value.GetValueOrDefault(), format, destination, out charsWritten);
    }

#endif
}
