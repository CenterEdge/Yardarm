using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RootNamespace.Serialization.Json;

namespace Yardarm.NodaTime.UnitTests.Serialization.Json;

public class YardarmNodaTimeJsonConverterFactoryTests
{
    #region OffsetTime

    [Fact]
    public void CanConvert_OffsetTime_ReturnsTrue()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();
        Assert.True(factory.CanConvert(typeof(OffsetTime)));
    }

    [Fact]
    public void CanConvert_NullableOffsetTime_ReturnsTrue()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();
        Assert.True(factory.CanConvert(typeof(OffsetTime?)));
    }

    [Fact]
    public void Convert_OffsetTime_ReturnsOffsetWithMinutes()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime>>(factory.CreateConverter(typeof(OffsetTime), JsonSerializerOptions.Default));

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, new OffsetTime(LocalTime.Midnight, Offset.FromHours(1)), JsonSerializerOptions.Default);
            writer.Flush();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("\"00:00:00\\u002B01:00\"", json);
    }

    [Fact]
    public void Convert_OffsetTime_ReadsWithMinutes()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime>>(factory.CreateConverter(typeof(OffsetTime), JsonSerializerOptions.Default));

        var json = "\"00:00:00\\u002B01:00\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        var result = converter.Read(ref reader, typeof(OffsetTime), JsonSerializerOptions.Default);

        Assert.Equal(new OffsetTime(LocalTime.Midnight, Offset.FromHours(1)), result);
    }

    [Fact]
    public void Convert_OffsetTime_ReadsZ()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime>>(factory.CreateConverter(typeof(OffsetTime), JsonSerializerOptions.Default));

        var json = "\"00:00:00Z\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        var result = converter.Read(ref reader, typeof(OffsetTime), JsonSerializerOptions.Default);

        Assert.Equal(new OffsetTime(LocalTime.Midnight, Offset.Zero), result);
    }

    #endregion

    #region NullableOffsetTime

    [Fact]
    public void Convert_NullableOffsetTime_ReturnsOffsetWithMinutes()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime?>>(factory.CreateConverter(typeof(OffsetTime?), JsonSerializerOptions.Default));

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, new OffsetTime(LocalTime.Midnight, Offset.FromHours(1)), JsonSerializerOptions.Default);
            writer.Flush();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("\"00:00:00\\u002B01:00\"", json);
    }

    [Fact]
    public void Convert_NullableOffsetTime_WritesNull()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime?>>(factory.CreateConverter(typeof(OffsetTime?), JsonSerializerOptions.Default));

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, null, JsonSerializerOptions.Default);
            writer.Flush();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("null", json);
    }

    [Fact]
    public void Convert_NullableOffsetTime_ReadsWithMinutes()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime?>>(factory.CreateConverter(typeof(OffsetTime?), JsonSerializerOptions.Default));

        var json = "\"00:00:00\\u002B01:00\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        var result = converter.Read(ref reader, typeof(OffsetTime?), JsonSerializerOptions.Default);

        Assert.NotNull(result);
        Assert.Equal(new OffsetTime(LocalTime.Midnight, Offset.FromHours(1)), result.Value);
    }

    [Fact]
    public void Convert_NullableOffsetTime_ReadsZ()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime?>>(factory.CreateConverter(typeof(OffsetTime?), JsonSerializerOptions.Default));

        var json = "\"00:00:00Z\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        var result = converter.Read(ref reader, typeof(OffsetTime?), JsonSerializerOptions.Default);

        Assert.NotNull(result);
        Assert.Equal(new OffsetTime(LocalTime.Midnight, Offset.Zero), result.Value);
    }

    [Fact]
    public void Convert_NullableOffsetTime_ReadsNull()
    {
        var factory = new YardarmNodaTimeJsonConverterFactory();

        var converter = Assert.IsAssignableFrom<JsonConverter<OffsetTime?>>(factory.CreateConverter(typeof(OffsetTime?), JsonSerializerOptions.Default));

        var json = "null"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        var result = converter.Read(ref reader, typeof(OffsetTime?), JsonSerializerOptions.Default);

        Assert.Null(result);
    }

    #endregion
}
