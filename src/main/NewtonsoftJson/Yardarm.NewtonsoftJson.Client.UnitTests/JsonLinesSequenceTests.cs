using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RootNamespace.Serialization;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.NewtonsoftJson.Client.UnitTests;

public class JsonLinesSequenceTests
{
    private static JsonSerializerSettings CreateSettings(Formatting formatting = Formatting.None) =>
        new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = formatting
        };

    [Fact]
    public async Task SerializeSequence_WritesOneRecordPerLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var content = serializer.SerializeSequence(AsyncItems(new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" }),
            "application/jsonl");

        // Assert

        content.Headers.ContentType!.ToString().Should().Be("application/jsonl; charset=utf-8");
        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("{\"id\":1,\"name\":\"a\"}\n{\"id\":2,\"name\":\"b\"}\n");
    }

    [Fact]
    public async Task SerializeSequence_IndentedSettings_WritesOneRecordPerLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings(Formatting.Indented));

        // Act

        var content = serializer.SerializeSequence(AsyncItems(new Item { Id = 1, Name = "a" }, new Item { Id = 2, Name = "b" }),
            "application/jsonl");

        // Assert

        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("{\"id\":1,\"name\":\"a\"}\n{\"id\":2,\"name\":\"b\"}\n");
    }

    [Theory]
    [InlineData("{\"id\":1}\n{\"id\":2}\n")]
    [InlineData("{\"id\":1}\n{\"id\":2}")]
    [InlineData("{\"id\":1}\r\n{\"id\":2}\r\n")]
    [InlineData("\n{\"id\":1}\n\n\r\n{\"id\":2}\n\n")]
    [InlineData("﻿{\"id\":1}\n{\"id\":2}\n")]
    [InlineData("/* first */\n{\"id\":1}\n// second\n{\"id\":2}\n")]
    public async Task DeserializeSequenceAsync_ReadsEachRecord(string body)
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(
            new StringContent(body, new UTF8Encoding(false)), cancellationToken: TestContext.Current.CancellationToken));

        // Assert

        result.Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
    }

    [Fact]
    public async Task DeserializeSequenceAsync_Primitives_ReadsEachRecord()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

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
    public async Task DeserializeSequenceAsync_DateOffsetString_MatchesDeserialize()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());
        const string record = "{\"when\":\"2024-01-02T03:04:05+05:00\"}";

        // Act

        var sequenceResult = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(new StringContent(record),
            cancellationToken: TestContext.Current.CancellationToken));
        var singleResult = await serializer.DeserializeAsync<Item>(new StringContent(record),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        sequenceResult.Should().ContainSingle().Which.When.Should().Be(singleResult.When);
        singleResult.When.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(5)));
    }

    [Fact]
    public async Task DeserializeSequenceAsync_Empty_NoItems()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var result = await ToListAsync(serializer.DeserializeSequenceAsync<Item>(new StringContent(""),
            cancellationToken: TestContext.Current.CancellationToken));

        // Assert

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeserializeSequenceAsync_Canceled_Throws()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());
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
    public async Task RegistrySequenceMethods_UseSequenceSerializer()
    {
        // Arrange

        var registry = new TypeSerializerRegistry().Add(JsonLinesTypeSerializer.SupportedMediaTypes,
            new JsonLinesTypeSerializer(CreateSettings()));
        var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

        // Act

        var content = registry.SerializeSequence(items, "application/x-ndjson");
        var buffered = new ByteArrayContent(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        buffered.Headers.ContentType = content.Headers.ContentType;
        var result = await registry.DeserializeSequenceToListAsync<Item>(buffered,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        result.Should().BeEquivalentTo(items);
    }

    private static async IAsyncEnumerable<T> AsyncItems<T>(params T[] items)
    {
        foreach (T item in items)
        {
            await Task.Yield();
            yield return item;
        }
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? When { get; set; }
    }
}
