using RootNamespace.Serialization.Literals;
using RootNamespace.Serialization.Literals.Converters;

namespace Yardarm.NodaTime.UnitTests.Serialization.Literals.Converters;

public class NodaPatternConverterTests
{
    #region Serialize

    [Fact]
    public void Serialize_OffsetDateTime_ReturnsString()
    {
        // Act

        string result = NodaLiteralConverters.OffsetDateTimeConverter.Write(
            new OffsetDateTime(new LocalDateTime(2020, 1, 2, 3, 4, 5), Offset.FromHours(-4)),
            null);

        // Assert

        result.Should().Be("2020-01-02T03:04:05-04:00");
    }

    [Fact]
    public void Serialize_OffsetDateTimeMillis_ReturnsString()
    {
        // Act

        string result = NodaLiteralConverters.OffsetDateTimeConverter.Write(
            new OffsetDateTime(new LocalDateTime(2020, 1, 2, 3, 4, 5, 123), Offset.FromHours(-4)),
            null);

        // Assert

        result.Should().Be("2020-01-02T03:04:05.123-04:00");
    }

    [Theory]
    [InlineData("date")]
    [InlineData("full-date")]
    public void Serialize_LocalDate_ReturnsString(string format)
    {
        // Act

        string result = NodaLiteralConverters.LocalDateConverter.Write(
            new LocalDate(2020, 1, 2),
            format);

        // Assert

        result.Should().Be("2020-01-02");
    }

    [Fact]
    public void Serialize_LocalDateTime_ReturnsString()
    {
        // Act

        string result = NodaLiteralConverters.LocalDateTimeConverter.Write(
            new LocalDateTime(2020, 1, 2, 5, 30, 45),
            "local-date-time");

        // Assert

        result.Should().Be("2020-01-02T05:30:45");
    }

    [Theory]
    [InlineData("partial-time")]
    public void Serialize_LocalTime_ReturnsString(string format)
    {
        // Act

        string result = NodaLiteralConverters.LocalTimeConverter.Write(
            new LocalTime(3, 4, 5),
            format);

        // Assert

        result.Should().Be("03:04:05");
    }

    [Theory]
    [InlineData("partial-time")]
    public void Serialize_LocalTimeMillis_ReturnsString(string format)
    {
        // Act

        string result = NodaLiteralConverters.LocalTimeConverter.Write(
            new LocalTime(3, 4, 5, 123),
            format);

        // Assert

        result.Should().Be("03:04:05.123");
    }

    [Theory]
    [InlineData("partial-time")]
    public void Serialize_OffsetTime_ReturnsString(string format)
    {
        // Act

        string result = NodaLiteralConverters.OffsetTimeConverter.Write(
            new OffsetTime(new LocalTime(3, 4, 5), Offset.FromHours(-4)),
            format);

        // Assert

        result.Should().Be("03:04:05-04:00");
    }

    [Theory]
    [InlineData("partial-time")]
    public void Serialize_OffsetTimeMillis_ReturnsString(string format)
    {
        // Act

        string result = NodaLiteralConverters.OffsetTimeConverter.Write(
            new OffsetTime(new LocalTime(3, 4, 5, 123), Offset.FromHours(-4)),
            format);

        // Assert

        result.Should().Be("03:04:05.123-04:00");
    }

    #endregion

    #region Deserialize

    [Theory]
    [InlineData("date")]
    [InlineData("full-date")]
    public void Deserialize_LocalDate_ReturnsString(string format)
    {
        // Act

        var result = NodaLiteralConverters.LocalDateConverter.Read(
            "2020-01-02",
            format);

        // Assert

        result.Should().Be(new LocalDate(year: 2020, 01, 02));
    }

    [Fact]
    public void Deserialize_LocalDateTime_DeserializesString()
    {
        // Act

        var result = NodaLiteralConverters.LocalDateTimeConverter.Read(
            "2020-01-02T05:30:45",
            "local-date-time");

        // Assert

        result.Should().Be(new LocalDateTime(year: 2020, 01, 02, 05, 30, 45));
    }

    [Fact]
    public void Deserialize_OffsetDateTime_ReturnsString()
    {
        // Act

        var result = NodaLiteralConverters.OffsetDateTimeConverter.Read(
            "2020-01-02T03:04:05-04:00",
            null);

        // Assert

        result.Should().Be(new OffsetDateTime(new LocalDateTime(2020, 1, 2, 3, 4, 5), Offset.FromHours(-4)));
    }

    [Theory]
    [InlineData("partial-time")]
    public void Deserialize_LocalTime_ReturnsString(string format)
    {
        // Act

        var result = NodaLiteralConverters.LocalTimeConverter.Read(
            "13:01:02",
            format);

        // Assert

        result.Should().Be(new LocalTime(13, 1, 2));
    }

    [Theory]
    [InlineData("partial-time")]
    public void Deserialize_TimeOnlyWithMillis_ReturnsString(string format)
    {
        // Act

        var result = NodaLiteralConverters.LocalTimeConverter.Read(
            "13:01:02.234000",
            format);

        // Assert

        result.Should().Be(new LocalTime(13, 1, 2, 234));
    }

    #endregion
}
