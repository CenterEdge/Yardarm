using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.Client.UnitTests
{
    public class JsonSequenceSerializationTests
    {
        private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

        #region JsonLinesTypeSerializer.SerializeSequence

        [Fact]
        public async Task JsonLines_SerializeSequence_WritesOneRecordPerLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

            // Act

            var content = serializer.SerializeSequence<List<Item>, Item>(items, "application/jsonl");

            // Assert

            content.Headers.ContentType!.ToString().Should().Be("application/jsonl; charset=utf-8");
            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":\"a\"}\n{\"id\":2,\"name\":\"b\"}\n");
        }

        [Fact]
        public async Task JsonLines_SerializeSequenceIndentedOptions_WritesOneRecordPerLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            });
            Item[] items = [new() { Id = 1 }, new() { Id = 2 }];

            // Act

            var content = serializer.SerializeSequence<Item[], Item>(items, "application/jsonl");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":null}\n{\"id\":2,\"name\":null}\n");
        }

        [Fact]
        public async Task JsonLines_SerializeSequenceEnumerable_WritesOneRecordPerLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence<IEnumerable<int>, int>(Enumerable.Range(1, 3), "application/jsonl");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("1\n2\n3\n");
        }

        [Fact]
        public async Task JsonLines_SerializeSequenceEmptyOrNull_WritesNothing()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var empty = serializer.SerializeSequence<List<Item>, Item>([], "application/jsonl");
            var nullValue = serializer.SerializeSequence<List<Item>, Item>(null!, "application/jsonl");

            // Assert

            (await empty.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().BeEmpty();
            (await nullValue.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().BeEmpty();
        }

        [Fact]
        public async Task JsonLines_SerializeSequence_Utf8WithoutBom()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence<List<Item>, Item>([new() { Id = 1, Name = "é" }],
                "application/jsonl");

            // Assert

            (await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should()
                .Equal(Encoding.UTF8.GetBytes("{\"id\":1,\"name\":\"\\u00E9\"}\n"));
        }

        #endregion

        #region JsonLinesTypeSerializer.DeserializeSequenceAsync

        [Theory]
        [InlineData("{\"id\":1}\n{\"id\":2}\n")]
        [InlineData("{\"id\":1}\n{\"id\":2}")]
        [InlineData("{\"id\":1}\r\n{\"id\":2}\r\n")]
        [InlineData("\n{\"id\":1}\n\n\r\n{\"id\":2}\n\n")]
        [InlineData("\uFEFF{\"id\":1}\n{\"id\":2}\n")]
        public async Task JsonLines_DeserializeSequenceAsyncList_ReadsEachRecord(string body)
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<List<Item>, Item>(
                new StringContent(body, new UTF8Encoding(false)),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeOfType<List<Item>>();
            result.Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncArray_ReadsEachRecord()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<int[], int>(new StringContent("1\n2\n3\n"),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncEnumerable_ReadsEachRecord()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<IEnumerable<string>, string>(
                new StringContent("\"a\"\nnull\n"), cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().Equal("a", null);
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncEmpty_ReturnsEmpty()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<List<Item>, Item>(new StringContent(""),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeEmpty();
        }

        [Fact]
        public void JsonLines_DeserializeSequenceAsyncUnsupportedType_ThrowsNotSupportedException()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            Action hashSet = () => serializer.DeserializeSequenceAsync<HashSet<int>, int>(new StringContent("1\n"),
                cancellationToken: TestContext.Current.CancellationToken);
            Action readOnlyList = () => serializer.DeserializeSequenceAsync<IReadOnlyList<int>, int>(
                new StringContent("1\n"), cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            hashSet.Should().Throw<NotSupportedException>();
            readOnlyList.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncMalformed_ThrowsJsonException()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            Func<Task> action = async () => await serializer.DeserializeSequenceAsync<List<Item>, Item>(
                new StringContent("{\"id\":1}\n{\"id\":\n"), cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            await action.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncCanceled_Throws()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cts.Cancel();

            // Act

            Func<Task> action = async () => await serializer.DeserializeSequenceAsync<List<Item>, Item>(
                new StringContent("{\"id\":1}\n{\"id\":2}\n"), cancellationToken: cts.Token);

            // Assert

            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        #endregion

        #region Round trips

        [Fact]
        public async Task JsonLines_RegistrySequenceRoundTrip_ReturnsItems()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add(JsonLinesTypeSerializer.SupportedMediaTypes,
                new JsonLinesTypeSerializer(s_options));
            var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

            // Act

            var content = await BufferAsync(registry.SerializeSequence<List<Item>, Item>(items, "application/x-ndjson"));
            var result = await registry.DeserializeSequenceAsync<List<Item>, Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeEquivalentTo(items);
        }

        [Fact]
        public async Task Json_RegistrySequenceRoundTrip_UsesJsonArray()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add(JsonTypeSerializer.SupportedMediaTypes,
                new JsonTypeSerializer(s_options));
            var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

            // Act

            var content = await BufferAsync(registry.SerializeSequence<List<Item>, Item>(items, "application/json"));
            string body = await content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = await registry.DeserializeSequenceAsync<List<Item>, Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            body.Should().Be("[{\"id\":1,\"name\":\"a\"},{\"id\":2,\"name\":\"b\"}]");
            result.Should().BeEquivalentTo(items);
        }

        #endregion

        #region Helpers

        // Simulates a response, whose content is buffered before it is read
        private static async Task<HttpContent> BufferAsync(HttpContent content)
        {
            var buffered = new ByteArrayContent(
                await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            buffered.Headers.ContentType = content.Headers.ContentType;
            return buffered;
        }

        public class Item
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        #endregion
    }
}
