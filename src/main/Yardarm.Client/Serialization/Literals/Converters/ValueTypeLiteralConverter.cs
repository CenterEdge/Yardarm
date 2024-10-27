namespace RootNamespace.Serialization.Literals.Converters;

/// <summary>
/// Base type of literal converters for value types.
/// </summary>
internal abstract class ValueTypeLiteralConverter<T> : LiteralConverter<T> where T : struct
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
}
