using System;

namespace RootNamespace.Serialization.Literals.Converters;

/// <summary>
/// Base type of literal converters for value types.
/// </summary>
internal abstract class ValueTypeLiteralConverter<T> : LiteralConverter<T>, IValueTypeLiteralConverter
    where T : struct
{
    public sealed override T Read(string? value, string? format)
    {
        if (value is null)
        {
            return default;
        }

        return ReadCore(value, format);
    }

    protected abstract T ReadCore(string value, string? format);

    /// <summary>
    /// Creates a literal converter for the nullable version of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A new literal converter for the nullable version of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is already a nullable type.</exception>
    public virtual LiteralConverter<T?> CreateNullableConverter()
    {
        if (Nullable.GetUnderlyingType(typeof(T)) is not null)
        {
            ThrowHelper.ThrowInvalidOperationException($"Type '{typeof(T).FullName}' is already a nullable type.");
        }

        return new NullableLiteralConverter<T>(this);
    }

    /// <inheritdoc />
    LiteralConverter IValueTypeLiteralConverter.CreateNullableConverter() => CreateNullableConverter();
}
