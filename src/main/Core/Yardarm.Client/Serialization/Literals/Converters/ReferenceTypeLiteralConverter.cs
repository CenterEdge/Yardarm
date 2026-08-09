using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals.Converters;

internal abstract class ReferenceTypeLiteralConverter<T> : LiteralConverter<T?> where T : class
{
    [return: NotNullIfNotNull(nameof(value))]
    public override T? Read(string? value, string? format) =>
        value is null ? null : ReadCore(value, format);

    protected abstract T ReadCore(string value, string? format);

    public sealed override string Write(T? value, string? format) =>
        value is null ? "" : WriteCore(value, format);

    protected abstract string WriteCore(T value, string? format);

#if NET6_0_OR_GREATER

    public override bool TryWrite(T? value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        if (value is null)
        {
            charsWritten = 0;
            return true;
        }

        return TryWriteCore(value, format, destination, out charsWritten);
    }

    protected abstract bool TryWriteCore(T value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten);

#endif
}
