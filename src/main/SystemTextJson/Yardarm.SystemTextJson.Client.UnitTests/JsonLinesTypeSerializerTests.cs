using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.Client.UnitTests
{
    public class JsonLinesTypeSerializerTests
    {
        private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

        [Fact]
        public async Task Serialize_Single_WritesOneLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.Serialize(new Item { Id = 1, Name = "a" }, "application/jsonl");

            // Assert

            content.Headers.ContentType!.ToString().Should().Be("application/jsonl; charset=utf-8");
            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":\"a\"}\n");
        }

        [Fact]
        public async Task Serialize_IndentedOptions_WritesOneLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            });

            // Act

            var content = serializer.Serialize(new Item { Id = 1, Name = "a" }, "application/jsonl");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":\"a\"}\n");
        }

        [Fact]
        public async Task Serialize_List_WritesOneArrayRecord()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.Serialize(new[] { 1, 2 }, "application/jsonl");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("[1,2]\n");
        }

        [Theory]
        [InlineData("{\"id\":1,\"name\":\"a\"}")]
        [InlineData("{\"id\":1,\"name\":\"a\"}\n")]
        [InlineData("{\"id\":1,\"name\":\"a\"}\r\n")]
        public async Task DeserializeAsync_Single_ReadsRecord(string body)
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeAsync<Item>(new StringContent(body),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeEquivalentTo(new Item { Id = 1, Name = "a" });
        }

        [Theory]
        [InlineData("{\"id\":1}\n{\"id\":2}\n")]
        [InlineData("")]
        public async Task DeserializeAsync_NotOneRecord_ThrowsJsonException(string body)
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            Func<Task> action = async () => await serializer.DeserializeAsync<Item>(
                new StringContent(body, new UTF8Encoding(false)),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            await action.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public void CreateDefault_SharesOptionsWithJsonTypeSerializer()
        {
            // Act

            var jsonLinesOptions = GetOptions(JsonLinesTypeSerializer.CreateDefault());
            var jsonOptions = GetOptions(JsonTypeSerializer.CreateDefault());

            // Assert

            jsonLinesOptions.Should().BeSameAs(jsonOptions);
        }

        private static JsonSerializerOptions GetOptions(object serializer) =>
            (JsonSerializerOptions)serializer.GetType()
                .GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(serializer)!;

        public class Item
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
