using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.Schema;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Schema;

public class StringSchemaGeneratorTests
{
    [Theory]
    [InlineData("base64", "byte[]")]
    [InlineData("base64url", "string")]
    [InlineData("custom", "string")]
    public void TypeInfo_ContentEncoding_ReturnsExpectedType(string contentEncoding, string expectedType)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            ContentEncoding = contentEncoding,
            Format = "binary"
        };

        GetTypeName(new StringSchemaGenerator(schema.CreateRoot("schema"), CreateContext(), null))
            .Should().Be(expectedType);
    }

    [Fact]
    public void Create_UntypedBase64Content_ReturnsByteArray()
    {
        var schema = new OpenApiSchema { ContentEncoding = "base64" };
        var result = new DefaultSchemaGeneratorFactory(CreateContext()).Create(schema.CreateRoot("schema"), null);

        result.Should().BeOfType<StringSchemaGenerator>();
        GetTypeName(result).Should().Be("byte[]");
    }

    [Fact]
    public void Create_RawBinaryWithExplicitMediaType_ReturnsStream()
    {
        var schema = new OpenApiSchema { ContentMediaType = "image/png" };
        var result = new DefaultSchemaGeneratorFactory(CreateContext()).Create(schema.CreateRoot("schema"), null);

        result.Should().BeOfType<StringSchemaGenerator>();
        GetTypeName(result).Should().Be("global::System.IO.Stream");
    }

    [Fact]
    public void Create_RawBinaryWithInferredMediaType_ReturnsStringGenerator()
    {
        var schema = new OpenApiSchema();
        var mediaType = new OpenApiMediaType { Schema = schema };
        var schemaElement = mediaType.CreateRoot("application/octet-stream").GetSchema()!;
        var result = new DefaultSchemaGeneratorFactory(CreateContext()).Create(schemaElement, null);

        result.Should().BeOfType<StringSchemaGenerator>();
        GetTypeName(result).Should().Be("global::System.IO.Stream");
    }

    [Fact]
    public void Create_UntypedSchemaWithoutContentInformation_ReturnsDynamicGenerator()
    {
        var result = new DefaultSchemaGeneratorFactory(CreateContext())
            .Create(new OpenApiSchema().CreateRoot("schema"), null);

        result.Should().BeOfType<DynamicSchemaGenerator>();
    }

    [Theory]
    [InlineData("byte", "byte[]")]
    [InlineData("binary", "global::System.IO.Stream")]
    public void TypeInfo_LegacyFormat_ReturnsExpectedType(string format, string expectedType)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = format };

        GetTypeName(new StringSchemaGenerator(schema.CreateRoot("schema"), CreateContext(), null))
            .Should().Be(expectedType);
    }

    private static GenerationContext CreateContext() =>
        new(new YardarmGenerationSettings().BuildServiceProvider(new OpenApiDocument()));

    private static string GetTypeName(ITypeGenerator generator) =>
        generator.TypeInfo.Name.ToFullString();
}
