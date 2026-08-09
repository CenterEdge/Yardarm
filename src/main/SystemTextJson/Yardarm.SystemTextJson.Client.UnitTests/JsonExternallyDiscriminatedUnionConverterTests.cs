using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using RootNamespace.Internal;
using RootNamespace.Models;
using RootNamespace.Serialization.Json;
using Xunit;

#nullable enable

namespace Yardarm.SystemTextJson.Client.UnitTests;

public class JsonExternallyDiscriminatedUnionConverterTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    #region Deserialize

    [Fact]
    public void Deserialize_CaseA()
    {
        // Arrange

        const string json = """
        {
            "caseA": {
                "name": "Test"
            }
        }
        """;

        // Act

        var result = JsonSerializer.Deserialize<TestUnion>(json, s_options);

        // Assert

        var value = Assert.IsType<CaseA>(result.Value);
        Assert.Equal("Test", value.Name);
    }

    [Fact]
    public void Deserialize_CaseB()
    {
        // Arrange

        const string json = """
        {
            "caseB": {
                "id": 123
            }
        }
        """;

        // Act

        var result = JsonSerializer.Deserialize<TestUnion>(json, s_options);

        // Assert

        var value = Assert.IsType<CaseB>(result.Value);
        Assert.Equal(123, value.Id);
    }

    [Fact]
    public void Deserialize_CaseBChild()
    {
        // Arrange

        const string json = """
        {
            "caseB": {
                "type": "caseBChild",
                "id": 123,
                "name": "Test"
            }
        }
        """;

        // Act

        var result = JsonSerializer.Deserialize<TestUnion>(json, s_options);

        // Assert

        var value = Assert.IsType<CaseBChild>(result.Value);
        Assert.Equal(123, value.Id);
        Assert.Equal("Test", value.Name);
    }

    [Theory]
    [InlineData("""
        {
            "caseA": {
                "name": "Test"
            },
            "ignored": true,
            "metadata": {
                "source": "api"
            }
        }
        """)]
    [InlineData("""
        {
            "ignored": true,
            "caseA": {
                "name": "Test"
            },
            "metadata": {
                "source": "api"
            }
        }
        """)]
    [InlineData("""
        {
            "ignored": true,
            "metadata": {
                "source": "api"
            },
            "caseA": {
                "name": "Test"
            }
        }
        """)]
    public void Deserialize_CaseA_WithAdditionalProperties_IgnoresExtras(string json)
    {
        // Act

        var result = JsonSerializer.Deserialize<TestUnion>(json, s_options);

        // Assert

        var value = Assert.IsType<CaseA>(result.Value);
        Assert.Equal("Test", value.Name);
    }

    [Fact]
    public void Deserialize_CaseA_InOuterDto()
    {
        // Arrange

        const string json = """
        {
            "union": {
                "caseA": {
                    "name": "Test"
                }
            },
            "otherValue": "foobar"
        }
        """;

        // Act

        var result = JsonSerializer.Deserialize<OuterDto>(json, s_options);

        // Assert

        Assert.NotNull(result);
        var value = Assert.IsType<CaseA>(result.Union.Value);
        Assert.Equal("Test", value.Name);
        Assert.Equal("foobar", result.OtherValue);
    }

    [Fact]
    public void Deserialize_EmptyObject_InOuterDtoWithUnknownCase_Returned()
    {
        // Arrange

        const string json = """
        {
            "union": {
            },
            "otherValue": "foobar"
        }
        """;

        // Act

        var result = JsonSerializer.Deserialize<OuterWithUnknownDto>(json, s_options);

        // Assert

        Assert.NotNull(result);
        Assert.IsType<UnknownCase>(result.Union.Value);
        Assert.Equal("foobar", result.OtherValue);
    }

    [Theory]
    [InlineData("""
        {
            "caseC": {
                "value": "unknown"
            }
        }
        """)]
    [InlineData("{}")]
    public void Deserialize_NoUnknownCase_Throws(string json)
    {
        // Act/Assert

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestUnion>(json, s_options));
    }

    [Theory]
    [InlineData("""
        {
            "caseC": {
                "value": "unknown"
            }
        }
        """)]
    [InlineData("""
        {
        }
        """)]
    public void Deserialize_HasUnknownCase_Returned(string json)
    {
        // Act

        var result = JsonSerializer.Deserialize<TestUnionWithUnknown>(json, s_options);

        // Assert

        Assert.IsType<UnknownCase>(result.Value);
    }

    [Fact]
    public void Deserialize_NullUnion_ReturnsNull()
    {
        // Act

        var result = JsonSerializer.Deserialize<TestUnion?>("null", s_options);

        // Assert

        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_NullInnerValue_Throws()
    {
        // Arrange

        const string json = """
        {
            "caseA": null
        }
        """;

        // Act

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestUnion>(json, s_options));

        // Assert

        Assert.Contains("Invalid JSON for union type", ex.Message);
    }

    #endregion

    #region Serialize

    [Fact]
    public void Serialize_CaseA()
    {
        // Arrange

        var union = new TestUnion(new CaseA()
        {
            Name = "Test"
        });

        // Act

        string result = JsonSerializer.Serialize(union, s_options);

        // Assert

        Assert.Equal("{\"caseA\":{\"name\":\"Test\"}}", result);
    }

    [Fact]
    public void Serialize_CaseB()
    {
        // Arrange

        var union = new TestUnion(new CaseB()
        {
            Id = 123
        });

        // Act

        string result = JsonSerializer.Serialize(union, s_options);

        // Assert

        Assert.Equal("{\"caseB\":{\"id\":123}}", result);
    }

    [Fact]
    public void Serialize_CaseBChild()
    {
        // Arrange

        var union = new TestUnion(new CaseBChild()
        {
            Id = 123,
            Name = "Test"
        });

        // Act

        string result = JsonSerializer.Serialize(union, s_options);

        // Assert

        Assert.Equal("{\"caseB\":{\"type\":\"caseBChild\",\"name\":\"Test\",\"id\":123}}", result);
    }

    [Fact]
    public void Serialize_NullUnion_WritesNull()
    {
        // Arrange

        TestUnion? union = null;

        // Act

        string result = JsonSerializer.Serialize(union, s_options);

        // Assert

        Assert.Equal("null", result);
    }

    [Fact]
    public void Serialize_UnknownCase()
    {
        // Arrange

        var union = new TestUnionWithUnknown(UnknownCase.Value);

        // Act

        string result = JsonSerializer.Serialize(union, s_options);

        // Assert

        Assert.Equal("{}", result);
    }

    [Fact]
    public void Serialize_Default_Throws()
    {
        // Act

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(default(TestUnion), s_options));

        // Assert

        Assert.Contains("may not contain a null value", ex.Message);
    }

    #endregion

    #region Helpers

    private class CaseA
    {
        public string? Name { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(CaseBChild), "caseBChild")]
    private class CaseB
    {
        public int Id { get; set; }
    }

    private class CaseBChild : CaseB
    {
        public string? Name { get; set; }
    }

    [Union]
    [JsonConverter(typeof(JsonExternallyDiscriminatedUnionConverter<TestUnion>))]
    private readonly struct TestUnion : IUnion
    {
        public object? Value { get; }

        [UnionCaseName("caseA")]
        public TestUnion(CaseA value) => Value = value;

        [UnionCaseName("caseB")]
        public TestUnion(CaseB value) => Value = value;
    }

    [Union]
    [JsonConverter(typeof(JsonExternallyDiscriminatedUnionConverter<TestUnionWithUnknown>))]
    private readonly struct TestUnionWithUnknown : IUnion
    {
        public object? Value { get; }

        [UnionCaseName("caseA")]
        public TestUnionWithUnknown(CaseA value) => Value = value;

        [UnionCaseName("caseB")]
        public TestUnionWithUnknown(CaseB value) => Value = value;

        public TestUnionWithUnknown(UnknownCase value) => Value = value;
    }

    private sealed class OuterDto
    {
        public TestUnion Union { get; set; }

        public string? OtherValue { get; set; }
    }

    private sealed class OuterWithUnknownDto
    {
        public TestUnionWithUnknown Union { get; set; }

        public string? OtherValue { get; set; }
    }

    #endregion
}
