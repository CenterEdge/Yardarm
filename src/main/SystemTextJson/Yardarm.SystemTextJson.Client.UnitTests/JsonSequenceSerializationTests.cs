using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

        #region JsonLinesTypeSerializer

        [Fact]
        public async Task JsonLines_SerializeSequence_WritesOneRecordPerLine()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence(AsyncItems(new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" }),
                "application/jsonl");

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

            // Act

            var content = serializer.SerializeSequence(AsyncItems(new Item { Id = 1 }, new Item { Id = 2 }),
                "application/jsonl");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":null}\n{\"id\":2,\"name\":null}\n");
        }

        [Fact]
        public async Task JsonLines_SerializeSequenceEmpty_WritesNothing()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence(AsyncItems<Item>(), "application/jsonl");

            // Assert

            (await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().BeEmpty();
        }

        [Theory]
        [InlineData("{\"id\":1}\n{\"id\":2}\n")]
        [InlineData("{\"id\":1}\n{\"id\":2}")]
        [InlineData("{\"id\":1}\r\n{\"id\":2}\r\n")]
        [InlineData("\n{\"id\":1}\n\n\r\n{\"id\":2}\n\n")]
        [InlineData("﻿{\"id\":1}\n{\"id\":2}\n")]
        public async Task JsonLines_DeserializeSequenceAsync_ReadsEachRecord(string body)
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(
                new StringContent(body, new UTF8Encoding(true)), cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncPrimitives_ReadsEachRecord()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var ints = await ToListAsync(serializer.DeserializeSequenceAsync<int>(new StringContent("1\n2\n3\n"),
                cancellationToken: TestContext.Current.CancellationToken));
            var strings = await ToListAsync(serializer.DeserializeSequenceAsync<string>(new StringContent("\"a\"\nnull\n"),
                cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            ints.Should().Equal(1, 2, 3);
            strings.Should().Equal("a", null);
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncEmpty_NoItems()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(new StringContent(""),
                cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncMalformed_ThrowsJsonException()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            Func<Task> action = () => ToListAsync(serializer.DeserializeSequenceAsync<Item>(
                new StringContent("{\"id\":1}\n{\"id\":\n"), cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            await action.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task JsonLines_DeserializeSequenceAsyncCanceled_Throws()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            Func<Task> action = async () =>
            {
                await foreach (var _ in serializer.DeserializeSequenceAsync<Item>(
                                   new StringContent("{\"id\":1}\n{\"id\":2}\n"), cancellationToken: cts.Token))
                {
                    cts.Cancel();
                }
            };

            // Assert

            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task JsonLines_SequenceRoundTrip_ReturnsItems()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            var items = new[] { new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" } };

            // Act

            var content = serializer.SerializeSequence(AsyncItems(items), "application/x-ndjson");
            content = await BufferAsync(content);
            var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(content,
                cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEquivalentTo(items);
        }

        [Fact]
        public async Task JsonLines_RegistrySequenceMethods_UseSequenceSerializer()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add(JsonLinesTypeSerializer.SupportedMediaTypes,
                new JsonLinesTypeSerializer(s_options));
            var items = new List<Item> { new() { Id = 1 }, new() { Id = 2 } };

            // Act

            var content = registry.SerializeSequence(items, "application/jsonl");
            content = await BufferAsync(content);
            var result = await registry.DeserializeListAsync<Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("{\"id\":1,\"name\":null}\n{\"id\":2,\"name\":null}\n");
            result.Should().BeEquivalentTo(items);
        }

        #endregion

        #region JsonTypeSerializer

        [Fact]
        public async Task Json_SerializeSequence_WritesArray()
        {
            // Arrange

            var serializer = new JsonTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence(AsyncItems(new Item { Id = 1, Name = "a" }, new Item { Id = 2 }),
                "application/json");

            // Assert

            content.Headers.ContentType!.ToString().Should().Be("application/json; charset=utf-8");
            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
                .Be("[{\"id\":1,\"name\":\"a\"},{\"id\":2,\"name\":null}]");
        }

        [Fact]
        public async Task Json_SerializeSequenceEmpty_WritesEmptyArray()
        {
            // Arrange

            var serializer = new JsonTypeSerializer(s_options);

            // Act

            var content = serializer.SerializeSequence(AsyncItems<Item>(), "application/json");

            // Assert

            (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("[]");
        }

        [Fact]
        public async Task Json_DeserializeSequenceAsync_ReadsArrayElements()
        {
            // Arrange

            var serializer = new JsonTypeSerializer(s_options);

            // Act

            var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(
                new StringContent("[{\"id\":1},{\"id\":2}]"), cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
        }

        [Fact]
        public async Task Json_SequenceRoundTrip_ReturnsItems()
        {
            // Arrange

            var serializer = new JsonTypeSerializer(s_options);
            var items = new[] { new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" } };

            // Act

            var content = serializer.SerializeSequence(AsyncItems(items), "application/json");
            content = await BufferAsync(content);
            var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(content,
                cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEquivalentTo(items);
        }

        #endregion

        #region Helpers

        private static async IAsyncEnumerable<T> AsyncItems<T>(params T[] items)
        {
            foreach (T item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        // Simulates a response, whose content is buffered before it is read
        private static async Task<HttpContent> BufferAsync(HttpContent content)
        {
            var buffered = new ByteArrayContent(
                await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            buffered.Headers.ContentType = content.Headers.ContentType;
            return buffered;
        }

        private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (T item in source)
            {
                list.Add(item);
            }

            return list;
        }

        public class Item
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        #endregion
    }
}
