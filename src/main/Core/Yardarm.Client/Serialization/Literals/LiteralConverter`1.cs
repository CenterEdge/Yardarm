using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Converts literals to strings for headers, query parameters, etc.
/// </summary>
/// <typeparam name="T">Type to convert</typeparam>
public abstract class LiteralConverter<T> : LiteralConverter
{
    /// <inheritdoc/>
    public sealed override Type Type { get; } = typeof(T);

    /// <summary>
    /// Read a value from a string.
    /// </summary>
    /// <param name="value">String to read.</param>
    /// <param name="format">Format from the OpenAPI specification.</param>
    /// <returns>Value parsed from the string.</returns>
    [return: NotNullIfNotNull(nameof(value))]
    public abstract T? Read(string? value, string? format);

    /// <summary>
    /// Write a value to a string.
    /// </summary>
    /// <param name="value">Value to write.</param>
    /// <param name="format">Format from the OpenAPI specification.</param>
    /// <returns>Value serialized as a string, or an empty string if null.</returns>
    public abstract string Write(T value, string? format);

#if NET6_0_OR_GREATER

    /// <summary>
    /// Try to write a value to a character buffer.
    /// </summary>
    /// <param name="value">Value to write.</param>
    /// <param name="format">Format from the OpenAPI specification.</param>
    /// <param name="destination">Buffer to write to.</param>
    /// <param name="charsWritten">Number of characters written to the buffer.</param>
    /// <returns>True if the value was successfully written, otherwise false.</returns>
    public virtual bool TryWrite(T value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        // Fallback implementation for LiteralConverter implementations that don't implement TryWrite.
        // It is recommended to override this method for performance reasons.

        string serialized = Write(value, format.Length > 0 ? new string(format) : null);

        if (!serialized.TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = serialized.Length;
        return true;
    }

#endif
}
