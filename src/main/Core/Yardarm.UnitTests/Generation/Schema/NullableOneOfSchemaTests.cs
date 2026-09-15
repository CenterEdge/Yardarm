using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Schema;

public class NullableOneOfSchemaTests
{
    [Theory]
    [InlineData("NullableOneOfInlineReference", "date")]
    [InlineData("NullableOneOfComponentReference", null)]
    [InlineData("NullableOneOfInlineValue", "date")]
    [InlineData("NullableOneOfComponentValue", null)]
    public async Task Get_NullableOneOfSchema_UsesNonNullSchemaType(string schemaName, string propertyName)
    {
        // Arrange

        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "swagger.json"));
        OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(stream, TestContext.Current.CancellationToken);
        var registry = document.CreateRegistry();
        ILocatedOpenApiElement<IOpenApiSchema> schema = document.Components.Schemas[schemaName].CreateRoot(schemaName);
        if (propertyName is not null)
        {
            schema = schema.GetProperties().Single(p => p.Key == propertyName);
        }

        // Act

        var generator = registry.Get(schema);

        // Assert

        schema.Element.Nullable.Should().BeTrue();
        schema.Element.TryGetNullableUnderlyingSchema(out var underlyingSchema).Should().BeTrue();
        underlyingSchema.Should().NotBeNull();
        generator.TypeInfo.Name.ToString().Should().Be("System.DateTime");
        generator.TypeInfo.IsGenerated.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_NullableOneOfReferences_ReferencesComponentSchemas()
    {
        // Arrange

        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "swagger.json"));

        // Act

        OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(stream, TestContext.Current.CancellationToken);
        var references = document.Components.Schemas["NullableOneOfReferences"]
            .CreateRoot("NullableOneOfReferences")
            .GetProperties()
            .Select(p => p.Element.GetReferenceId());

        // Assert

        references.Should().Equal(
            "NullableOneOfComponentReference",
            "NullableOneOfComponentValue");
    }

    [Fact]
    public async Task Generate_NullableOneOfObjectReferences_DoesNotRepeatReferencedDeclarations()
    {
        // Arrange

        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "swagger.json"));
        OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(stream, TestContext.Current.CancellationToken);
        var registry = document.CreateRegistry();
        var generator = new Yardarm.Generation.Schema.SchemaGenerator(document, registry);

        // Act

        var declarations = generator.Generate()
            .SelectMany(p => p.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Select(p => p.Identifier.ValueText);

        // Assert

        declarations.Should().ContainSingle(p => p == "NullableOneOfReferencedObject");
        declarations.Should().ContainSingle(p => p == "NullableOneOfInlineObjectReference");
    }

    [Fact]
    public void Get_NonNullableOneOfProperty_RemainsUnion()
    {
        // Arrange

        var propertySchema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaType.String },
                new OpenApiSchema { Type = JsonSchemaType.Integer }
            ]
        };
        var document = CreateDocument(propertySchema);
        var registry = document.CreateRegistry();
        var property = document.Components.Schemas["Parent"].CreateRoot("Parent").GetProperties().Single();

        // Act

        var generator = registry.Get(property);

        // Assert

        property.Element.Nullable.Should().BeFalse();
        generator.TypeInfo.IsGenerated.Should().BeTrue();
        generator.TypeInfo.Name.ToString().Should().Be("Yardarm.Sdk.Models.Parent.IChoiceModel");
    }

    [Fact]
    public void Get_MultipleNullOneOfProperty_RemainsUnion()
    {
        // Arrange

        var propertySchema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaType.Null },
                new OpenApiSchema { Type = JsonSchemaType.Null }
            ]
        };
        var document = CreateDocument(propertySchema);
        var registry = document.CreateRegistry();
        var property = document.Components.Schemas["Parent"].CreateRoot("Parent").GetProperties().Single();

        // Act

        var generator = registry.Get(property);

        // Assert

        property.Element.Nullable.Should().BeFalse();
        generator.TypeInfo.IsGenerated.Should().BeTrue();
    }

    private static OpenApiDocument CreateDocument(IOpenApiSchema propertySchema) =>
        new()
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["Parent"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["choice"] = propertySchema
                        }
                    }
                }
            }
        };
}

file static class OpenApiDocumentExtensions
{
    public static ITypeGeneratorRegistry<IOpenApiSchema> CreateRegistry(this OpenApiDocument document)
    {
        var settings = new YardarmGenerationSettings();
        return settings.BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();
    }
}
