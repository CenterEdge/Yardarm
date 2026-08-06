using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using RootNamespace.Internal;
using RootNamespace.Models;
using RootNamespace.Serialization.Json;
using Xunit;

#nullable enable

namespace Yardarm.NewtonsoftJson.UnitTests.Client;

public class JsonExternallyDiscriminatedUnionConverterTests
{
    private static readonly JsonSerializerSettings s_settings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
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

        var result = JsonConvert.DeserializeObject<TestUnion>(json, s_settings);

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
                "type": "caseB",
                "id": 123
            }
        }
        """;

        // Act

        var result = JsonConvert.DeserializeObject<TestUnion>(json, s_settings);

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

        var result = JsonConvert.DeserializeObject<TestUnion>(json, s_settings);

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

        var result = JsonConvert.DeserializeObject<TestUnion>(json, s_settings);

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

        var result = JsonConvert.DeserializeObject<OuterDto>(json, s_settings);

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

        var result = JsonConvert.DeserializeObject<OuterWithUnknownDto>(json, s_settings);

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

        Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<TestUnion>(json, s_settings));
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

        var result = JsonConvert.DeserializeObject<TestUnionWithUnknown>(json, s_settings);

        // Assert

        Assert.IsType<UnknownCase>(result.Value);
    }

    [Fact]
    public void Deserialize_NullUnion_ReturnsNull()
    {
        // Act

        var result = JsonConvert.DeserializeObject<TestUnion?>("null", s_settings);

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

        var ex = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<TestUnion>(json, s_settings));

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

        string result = JsonConvert.SerializeObject(union, s_settings);

        // Assert

        Assert.Equal("{\"caseA\":{\"name\":\"Test\"}}", result);
    }

    [Fact]
    public void Serialize_CaseB()
    {
        // Arrange

        var union = new TestUnion(new CaseB()
        {
            Id = 123,
            Type = "caseBChild"
        });

        // Act

        string result = JsonConvert.SerializeObject(union, s_settings);

        // Assert

        Assert.Equal("{\"caseB\":{\"type\":\"caseBChild\",\"id\":123}}", result);
    }

    [Fact]
    public void Serialize_CaseBChild()
    {
        // Arrange

        var union = new TestUnion(new CaseBChild()
        {
            Id = 123,
            Name = "Test",
            Type = "caseBChild"
        });

        // Act

        string result = JsonConvert.SerializeObject(union, s_settings);

        // Assert

        Assert.Equal("{\"caseB\":{\"name\":\"Test\",\"type\":\"caseBChild\",\"id\":123}}", result);
    }

    [Fact]
    public void Serialize_NullUnion_WritesNull()
    {
        // Arrange

        TestUnion? union = null;

        // Act

        string result = JsonConvert.SerializeObject(union, s_settings);

        // Assert

        Assert.Equal("null", result);
    }

    [Fact]
    public void Serialize_UnknownCase()
    {
        // Arrange

        var union = new TestUnionWithUnknown(UnknownCase.Value);

        // Act

        string result = JsonConvert.SerializeObject(union, s_settings);

        // Assert

        Assert.Equal("{}", result);
    }

    [Fact]
    public void Serialize_Default_Throws()
    {
        // Act

        var ex = Assert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(default(TestUnion), s_settings));

        // Assert

        Assert.Contains("may not contain a null value", ex.Message);
    }

    #endregion

    #region Helpers

    private class CaseA
    {
        public string? Name { get; set; }
    }


    [JsonConverter(typeof(DiscriminatorConverter),
        [
            "type",
            typeof(ICaseB),
            new object[] {
                "caseB", typeof(CaseB),
                "caseBChild", typeof(CaseBChild)
            }
        ])]
    private class CaseB : ICaseB
    {
        public string? Type { get; set; }
        public int Id { get; set; }
    }

    private interface ICaseB
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
