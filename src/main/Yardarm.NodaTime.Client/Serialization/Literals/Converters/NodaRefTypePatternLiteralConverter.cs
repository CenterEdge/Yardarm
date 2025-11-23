using System;
using System.Diagnostics.CodeAnalysis;
using NodaTime.Text;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class NodaRefTypePatternLiteralConverter<T>(IPattern<T> pattern, Action<T>? validator) : LiteralConverter<T?>
    where T : class
{
    public NodaRefTypePatternLiteralConverter(IPattern<T> pattern)
        : this(pattern, null)
    {
    }

    [return: NotNullIfNotNull(nameof(value))]
    public override T? Read(string? value, string? format)
    {
        if (value is null or { Length: 0 })
        {
            return default;
        }

        return pattern.Parse(value).Value;
    }

    public override string Write(T? value, string? format)
    {
        if (value is null)
        {
            return "";
        }

        validator?.Invoke(value);

        return pattern.Format(value);
    }
}
