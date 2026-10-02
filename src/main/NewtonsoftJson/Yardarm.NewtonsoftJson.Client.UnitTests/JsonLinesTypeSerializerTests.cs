using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.NewtonsoftJson.Client.UnitTests;

public class JsonLinesTypeSerializerTests
{
    private static JsonSerializerSettings CreateSettings(Formatting formatting = Formatting.None) =>
        new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = formatting
        };

    [Fact]
    public async Task Serialize_List_WritesOneRecordPerLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());
        var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

        // Act

        var content = serializer.Serialize(items, "application/jsonl");

        // Assert

        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("{\"id\":1,\"name\":\"a\"}\n{\"id\":2,\"name\":\"b\"}\n");
    }

    [Fact]
    public async Task Serialize_List_RoundTrips()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());
        var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

        // Act

        var content = serializer.Serialize(items, "application/x-ndjson");
        var result = await serializer.DeserializeAsync<List<Item>>(content,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        result.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Serialize_IndentedSettings_WritesOneRecordPerLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings(Formatting.Indented));
        var items = new List<Item> { new() { Id = 1, Name = "a" }, new() { Id = 2, Name = "b" } };

        // Act

        var content = serializer.Serialize(items, "application/jsonl");

        // Assert

        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("{\"id\":1,\"name\":\"a\"}\n{\"id\":2,\"name\":\"b\"}\n");
    }

    [Fact]
    public async Task Serialize_List_Utf8WithoutBom()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());
        var items = new List<Item> { new() { Id = 1, Name = "é" } };

        // Act

        var content = serializer.Serialize(items, "application/jsonl");
        byte[] bytes = await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert

        content.Headers.ContentType!.MediaType.Should().Be("application/jsonl");
        content.Headers.ContentType.CharSet.Should().Be("utf-8");
        bytes.Should().Equal(Encoding.UTF8.GetBytes("{\"id\":1,\"name\":\"é\"}\n"));
    }

    [Fact]
    public async Task Serialize_Single_WritesOneLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var content = serializer.Serialize(new Item { Id = 1, Name = "a" }, "application/jsonl");

        // Assert

        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("{\"id\":1,\"name\":\"a\"}\n");
    }

    [Fact]
    public async Task Serialize_PrimitiveList_WritesOneRecordPerLine()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var content = serializer.Serialize(new List<string> { "a", "b" }, "application/jsonl");

        // Assert

        (await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should()
            .Be("\"a\"\n\"b\"\n");
    }

    [Theory]
    [InlineData("{\"id\":1}\n{\"id\":2}\n")]
    [InlineData("{\"id\":1}\n{\"id\":2}")]
    [InlineData("{\"id\":1}\r\n{\"id\":2}\r\n")]
    [InlineData("\n{\"id\":1}\n\n\r\n{\"id\":2}\n\n")]
    [InlineData("﻿{\"id\":1}\n{\"id\":2}\n")]
    public async Task DeserializeAsync_List_ReadsEachRecord(string body)
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var result = await serializer.DeserializeAsync<List<Item>>(
            new StringContent(body, new UTF8Encoding(false)),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        result.Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
    }

    [Fact]
    public async Task DeserializeAsync_EmptyBody_EmptyList()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var result = await serializer.DeserializeAsync<List<Item>>(new StringContent(""),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeserializeAsync_Single_ReadsRecord()
    {
        // Arrange

        var serializer = new JsonLinesTypeSerializer(CreateSettings());

        // Act

        var result = await serializer.DeserializeAsync<Item>(new StringContent("{\"id\":1,\"name\":\"a\"}\r\n"),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert

        result.Should().BeEquivalentTo(new Item { Id = 1, Name = "a" });
    }

    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
