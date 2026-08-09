using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using RootNamespace.Models;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.Client.UnitTests;

public class JsonExtensibleEnumConverterTests
{
    [Fact]
    public void Serialize_ExtensibleEnum_ReturnsString()
    {
        // Act

        string result = JsonSerializer.Serialize(ExtensibleEnum.Case1);

        // Assert

        result.Should().Be("\"Case1\"");
    }

    [Fact]
    public void Serialize_NullableExtensibleEnum_ReturnsString()
    {
        // Act

        string result = JsonSerializer.Serialize<ExtensibleEnum?>(ExtensibleEnum.Case1);

        // Assert

        result.Should().Be("\"Case1\"");
    }

    [Fact]
    public void Serialize_NullableExtensibleEnum_ReturnsNull()
    {
        // Act

        string result = JsonSerializer.Serialize<ExtensibleEnum?>(null);

        // Assert

        result.Should().Be("null");
    }

    [Fact]
    public void Deserialize_ExtensibleEnum_ReturnsValue()
    {
        // Act
        ExtensibleEnum result = JsonSerializer.Deserialize<ExtensibleEnum>("\"Case1\"");

        // Assert
        result.Should().Be(ExtensibleEnum.Case1);
    }

    [Fact]
    public void Deserialize_NullableExtensibleEnum_ReturnsValue()
    {
        // Act
        ExtensibleEnum? result = JsonSerializer.Deserialize<ExtensibleEnum?>("\"Case1\"");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(ExtensibleEnum.Case1);
    }

    [Fact]
    public void Deserialize_NullableExtensibleEnum_ReturnsNull()
    {
        // Act
        ExtensibleEnum? result = JsonSerializer.Deserialize<ExtensibleEnum?>("null");

        // Assert
        result.Should().BeNull();
    }

    [JsonConverter(typeof(JsonExtensibleEnumConverter<ExtensibleEnum>))]
    private readonly record struct ExtensibleEnum(string Value) : IExtensibleEnum<ExtensibleEnum>
    {
        public static ExtensibleEnum Case1 { get; } = new(nameof(Case1));
        public static ExtensibleEnum Case2 { get; } = new(nameof(Case2));

        public static ExtensibleEnum Create(string value) => new(value);
    }
}
