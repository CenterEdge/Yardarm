using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// Handles reading and writing date-only JSON to and from <see cref="DateTime"/> properties.
/// </summary>
internal sealed class JsonDateConverter : JsonConverter<DateTime>
{
    private const string DateOnlyFormat = "yyyy-MM-dd";
    private const int FormatLength = 10;
    private const int MaxEscapedFormatLength = FormatLength * 6;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Read(ref reader);

    public static DateTime Read(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new InvalidOperationException(
                $"Cannot get the value of a token type '{reader.TokenType}' as a string.");
        }

        int valueLength = reader.HasValueSequence
            ? checked((int)reader.ValueSequence.Length)
            : reader.ValueSpan.Length;

        if (!IsInRangeInclusive(valueLength, FormatLength, MaxEscapedFormatLength))
        {
            throw new FormatException("The JSON value is not in a supported date format.");
        }

        // This isn't as optimal as parsing directly from bytes, but given the possibility of escaping and how
        // some of the System.Text.Json internals aren't available to us this is far simpler.
        string str = reader.GetString()!;

        if (!DateTime.TryParseExact(str, DateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var value))
        {
            throw new FormatException("The JSON value is not in a supported date format.");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => Write(writer, value);

    public static void Write(Utf8JsonWriter writer, DateTime value)
    {
#if NET6_0_OR_GREATER
        Span<char> buffer = stackalloc char[FormatLength];

        bool success = value.TryFormat(buffer, out int charsWritten, DateOnlyFormat, CultureInfo.InvariantCulture);
        Debug.Assert(success, "Unable to write to date to buffer.");

        writer.WriteStringValue(buffer.Slice(0, charsWritten));
#else
        writer.WriteStringValue(value.ToString(DateOnlyFormat));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInRangeInclusive(int value, int lowerBound, int upperBound)
        => (uint)(value - lowerBound) <= (uint)(upperBound - lowerBound);
}
