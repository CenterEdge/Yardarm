using System;
using Yardarm.Client.Internal;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class TimeOnlyLiteralConverter : ValueTypeLiteralConverter<TimeOnly>
{
    protected override TimeOnly ReadCore(string value, string? format)
    {
        ThrowHelper.ThrowIfNull(value);

        char firstChar = value[0];
        int firstSeparator = value.AsSpan().IndexOfAny('.', ':');
        if (!char.IsDigit(firstChar) || firstSeparator < 0 || value[firstSeparator] == '.')
        {
            // Note: TimeSpan.ParseExact permits leading whitespace, negative values
            // and numbers of days so we need to exclude these cases here.
            ThrowHelper.ThrowFormatException("The value is not in a supported TimeOnly format.");
        }

        return TimeOnly.FromTimeSpan(TimeSpan.ParseExact(value, "c", null));
    }

    public override string Write(TimeOnly value, string? format) =>
        value.ToTimeSpan().ToString("c");

    public override bool TryWrite(TimeOnly value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten) =>
        value.ToTimeSpan().TryFormat(destination, out charsWritten, "c");
}
