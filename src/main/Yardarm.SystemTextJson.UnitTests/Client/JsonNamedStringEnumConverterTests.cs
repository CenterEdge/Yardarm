using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.UnitTests.Client
{
    public class JsonNamedStringEnumConverterTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void Serialize_EnumValue_SerializesAsNamedString(TestEnum value, string expected)
        {
            // arrange

            var converterFactory = new JsonNamedStringEnumConverter<TestEnum>();

            var converter = (JsonConverter<TestEnum>) converterFactory.CreateConverter(typeof(TestEnum), JsonSerializerOptions.Default);

            using var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);

            // Act

            converter.Write(writer, value, JsonSerializerOptions.Default);
            writer.Flush();

            var result = Encoding.UTF8.GetString(stream.ToArray());

            // Assert

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Deserialize_EnumValue_DeserializesFromNamedString(TestEnum value, string expected)
        {
            // arrange

            var converterFactory = new JsonNamedStringEnumConverter<TestEnum>();

            var converter = (JsonConverter<TestEnum>) converterFactory.CreateConverter(typeof(TestEnum), JsonSerializerOptions.Default);

            var buffer = Encoding.UTF8.GetBytes(expected);
            var reader = new Utf8JsonReader(buffer);
            reader.Read();

            // Act

            var result = converter.Read(ref reader, typeof(TestEnum), JsonSerializerOptions.Default);

            // Assert

            Assert.Equal(value, result);
        }

        public static IEnumerable<object[]> TestCases()
        {
            yield return [TestEnum.None, "\"None\""];
            yield return [TestEnum.CamelCase, "\"camelCase\""];
        }

        public enum TestEnum
        {
            None = 0,

            [EnumMember(Value = "camelCase")]
            CamelCase = 1
        }
    }
}
