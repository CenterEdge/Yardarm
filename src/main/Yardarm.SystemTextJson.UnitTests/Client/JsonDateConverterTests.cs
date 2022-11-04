using System;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.UnitTests.Client
{
    public class JsonDateConverterTests
    {
        private static readonly UTF8Encoding NoBom = new(false);

        [Fact]
        public void Write_DateTime_WritesOnlyDate()
        {
            // Arrange

            var converter = new JsonDateConverter();

            var stream = new MemoryStream(128);
            var writer = new Utf8JsonWriter(stream);

            // Act

            converter.Write(writer, new DateTime(2022, 12, 31, 14, 02, 01), new JsonSerializerOptions());
            writer.Flush();

            // Assert

            var str = Encoding.UTF8.GetString(stream.ToArray());
            str.Should().Be("\"2022-12-31\"");
        }

        [Fact]
        public void Read_DateTime_ReadsOnlyDate()
        {
            // Arrange

            const string value = "\"2022-12-31\"";

            var converter = new JsonDateConverter();

            var reader = new Utf8JsonReader(NoBom.GetBytes(value));
            reader.Read();

            // Act

            var result = converter.Read(ref reader, typeof(DateTime), new JsonSerializerOptions());

            // Assert

            result.Should().Be(new DateTime(2022, 12, 31));
        }

        [Fact]
        public void Read_Null_InvalidOperationException()
        {
            // Arrange

            const string value = "null";

            var converter = new JsonDateConverter();


            // Act/Assert

            var action = () =>
            {
                var reader = new Utf8JsonReader(NoBom.GetBytes(value));
                reader.Read();

                return converter.Read(ref reader, typeof(DateTime), new JsonSerializerOptions());
            };

            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("\"2022-12-32\"")]
        [InlineData("\"2022-12-31T12:00:00Z\"")]
        public void Read_InvalidDate_FormatException(string value)
        {
            // Arrange

            var converter = new JsonDateConverter();


            // Act/Assert

            var action = () =>
            {
                var reader = new Utf8JsonReader(NoBom.GetBytes(value));
                reader.Read();

                return converter.Read(ref reader, typeof(DateTime), new JsonSerializerOptions());
            };

            action.Should().Throw<FormatException>();
        }
    }
}
