using System;
using NodaTime.Text;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class NodaPatternLiteralConverter<T>(IPattern<T> pattern, Action<T>? validator) : LiteralConverter<T>
    where T : struct
{
    public NodaPatternLiteralConverter(IPattern<T> pattern)
        : this(pattern, null)
    {
    }

    public override T Read(string? value, string? format)
    {
        if (value is null)
        {
            return default;
        }

        return pattern.Parse(value).Value;
    }

    public override string Write(T value, string? format)
    {
        validator?.Invoke(value);

        return pattern.Format(value);
    }
}
