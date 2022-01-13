using System;
using System.Text;
using System.Text.Json;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.UnitTests.Client
{
    public class JsonHelpersTests
    {
        private static UTF8Encoding Utf8Encoding { get; } = new(encoderShouldEmitUTF8Identifier: false);
        private static byte[] PropertyName { get; } = Utf8Encoding.GetBytes("type");

        #region GetDiscriminator

        [Fact]
        public void GetDiscriminator_NotPresent_Null()
        {
            // Arrange

            const string json = "{\"amount\":{\"bonusPoints\":100}}";

            var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
            reader.Read();

            // Act

            string result = JsonHelpers.GetDiscriminator(ref reader, PropertyName);

            // Assert

            Assert.Null(result);
        }

        [Fact]
        public void GetDiscriminator_TypeAtFront_Returns()
        {
            // Arrange

            const string json = "{\"type\":\"addValue\",\"amount\":{\"bonusPoints\":100}}";

            var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
            reader.Read();

            // Act

            string result = JsonHelpers.GetDiscriminator(ref reader, PropertyName);

            // Assert

            Assert.Equal("addValue", result);
        }

        [Fact]
        public void GetDiscriminator_TypeAtRear_Returns()
        {
            // Arrange

            const string json = "{\"amount\":100,\"type\":\"addValue\"}";

            var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
            reader.Read();

            // Act

            string result = JsonHelpers.GetDiscriminator(ref reader, PropertyName);

            // Assert

            Assert.Equal("addValue", result);
        }

        [Fact]
        public void GetDiscriminator_TypeAtRearWithObject_Returns()
        {
            // Arrange

            const string json = "{\"amount\":{\"bonusPoints\":100},\"type\":\"addValue\"}";

            var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
            reader.Read();

            // Act

            string result = JsonHelpers.GetDiscriminator(ref reader, PropertyName);

            // Assert

            Assert.Equal("addValue", result);
        }

        [Fact]
        public void GetDiscriminator_TypeAtRearWithArray_Returns()
        {
            // Arrange

            const string json = "{\"amount\":[100],\"type\":\"addValue\"}";

            var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
            reader.Read();

            // Act

            string result = JsonHelpers.GetDiscriminator(ref reader, PropertyName);

            // Assert

            Assert.Equal("addValue", result);
        }

        [Fact]
        public void GetDiscriminator_NotAnObject_JsonException()
        {
            // Arrange

            const string json = "100";

            // Act/Assert

            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Utf8Encoding.GetBytes(json));
                reader.Read();

                return JsonHelpers.GetDiscriminator(ref reader, PropertyName);
            });
        }

        #endregion
    }
}
