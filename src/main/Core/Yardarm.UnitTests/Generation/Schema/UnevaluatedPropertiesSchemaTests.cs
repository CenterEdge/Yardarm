using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Enrichment.Compilation;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Schema;

public class UnevaluatedPropertiesSchemaTests
{
    private const string AdditionalPropertiesName = "AdditionalProperties";

    [Fact]
    public void AllowsUndeclaredProperties_Default_IsTrue()
    {
        // Arrange

        var schema = new OpenApiSchema { Type = JsonSchemaType.Object };

        // Act

        var result = schema.AllowsUndeclaredProperties;

        // Assert

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("\"unevaluatedProperties\": { \"type\": \"string\" }", "string")]
    [InlineData("\"additionalProperties\": { \"type\": \"boolean\" }", "bool")]
    // JSON Schema: additionalProperties evaluates every remaining property, so an adjacent
    // unevaluatedProperties has no effect and additionalProperties decides
    [InlineData("\"additionalProperties\": { \"type\": \"boolean\" }, \"unevaluatedProperties\": { \"type\": \"string\" }", "bool")]
    [InlineData("\"additionalProperties\": { \"type\": \"boolean\" }, \"unevaluatedProperties\": false", "bool")]
    public async Task Generate_UndeclaredPropertiesSubschema_SetsDictionaryValueType(string keywords,
        string expectedValueType)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Widget": {
                "type": "object",
                "properties": { "name": { "type": "string" } },
                {{keywords}}
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Widget");

        // Assert

        GetAdditionalPropertiesValueType(classes["Widget"]).Should().Be(expectedValueType);
    }

    [Fact]
    public async Task Get_EmptyObjectWithUnevaluatedPropertiesSchema_IsTypedDictionary()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Names": { "type": "object", "unevaluatedProperties": { "type": "string" } }
            }
            """);
        var registry = CreateServices(document).GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

        // Act

        var generator = registry.Get(document.Components!.Schemas!["Names"].CreateRoot("Names"));

        // Assert

        generator.TypeInfo.IsGenerated.Should().BeFalse();
        generator.TypeInfo.Name.ToString().Should().Be("global::System.Collections.Generic.Dictionary<string,string>");
    }

    [Fact]
    public async Task Generate_AllOfDerivedWithUnevaluatedPropertiesSchema_ShadowsBaseAdditionalProperties()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Animal": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              },
              "Dog": {
                "type": "object",
                "allOf": [
                  { "$ref": "#/components/schemas/Animal" },
                  { "type": "object", "properties": { "breed": { "type": "string" } } }
                ],
                "unevaluatedProperties": { "type": "string" }
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Animal", "Dog");

        // Assert

        GetAdditionalPropertiesValueType(classes["Animal"]).Should().Be("object");
        GetAdditionalPropertiesValueType(classes["Dog"]).Should().Be("string");
        GetAdditionalProperties(classes["Dog"]).Modifiers.Should().Contain(p => p.IsKind(SyntaxKind.NewKeyword));
    }

    [Theory]
    [InlineData("false", false)]
    [InlineData("true", true)]
    [InlineData(null, true)]
    public async Task Generate_UnevaluatedProperties_AddsAdditionalPropertiesOnlyWhenOpen(string unevaluatedProperties,
        bool expectAdditionalProperties)
    {
        // Arrange

        string keyword = unevaluatedProperties is null
            ? ""
            : $", \"unevaluatedProperties\": {unevaluatedProperties}";
        OpenApiDocument document = await LoadAsync($$"""
            {
              "Widget": {
                "type": "object",
                "properties": { "name": { "type": "string" } }{{keyword}}
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Widget");

        // Assert

        HasAdditionalProperties(classes["Widget"]).Should().Be(expectAdditionalProperties);
        document.Components!.Schemas!["Widget"].AllowsUndeclaredProperties.Should().Be(expectAdditionalProperties);
    }

    [Fact]
    public async Task Generate_AdditionalPropertiesFalse_DoesNotAddAdditionalProperties()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Widget": {
                "type": "object",
                "properties": { "name": { "type": "string" } },
                "additionalProperties": false
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Widget");

        // Assert

        HasAdditionalProperties(classes["Widget"]).Should().BeFalse();
    }

    [Fact]
    public async Task Get_EmptyObjectWithUnevaluatedPropertiesFalse_IsNotDictionary()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Empty": { "type": "object", "unevaluatedProperties": false }
            }
            """);
        var registry = CreateServices(document).GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

        // Act

        var generator = registry.Get(document.Components!.Schemas!["Empty"].CreateRoot("Empty"));

        // Assert

        generator.TypeInfo.IsGenerated.Should().BeTrue();
        generator.TypeInfo.Name.ToString().Should().NotContain("Dictionary");
    }

    [Fact]
    public async Task Generate_AllOfOpenBaseAndClosedDerived_DerivedInheritsBaseAdditionalProperties()
    {
        // Arrange

        // Matches the producer: the base schema, which another schema extends with allOf, is open.
        // Only the derived leaf schema has unevaluatedProperties: false.
        OpenApiDocument document = await LoadAsync("""
            {
              "Animal": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              },
              "Dog": {
                "type": "object",
                "allOf": [
                  { "$ref": "#/components/schemas/Animal" },
                  { "type": "object", "properties": { "breed": { "type": "string" } } }
                ],
                "unevaluatedProperties": false
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Animal", "Dog");

        // Assert

        HasAdditionalProperties(classes["Animal"]).Should().BeTrue();
        classes["Dog"].BaseList!.Types.Select(p => p.Type.ToString()).Should().ContainSingle(p => p.EndsWith(".Animal"));
        HasAdditionalProperties(classes["Dog"]).Should().BeFalse(
            "a closed derived schema declares no dictionary of its own and inherits the base type's dictionary");
    }

    [Fact]
    public async Task Generate_AllOfOpenBaseAndOpenDerived_DerivedReusesBaseAdditionalProperties()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Animal": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              },
              "Dog": {
                "type": "object",
                "allOf": [
                  { "$ref": "#/components/schemas/Animal" },
                  { "type": "object", "properties": { "breed": { "type": "string" } } }
                ]
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Animal", "Dog");

        // Assert

        HasAdditionalProperties(classes["Animal"]).Should().BeTrue();
        HasAdditionalProperties(classes["Dog"]).Should().BeFalse("the matching base type dictionary is reused");
    }

    [Theory]
    [InlineData(", \"unevaluatedProperties\": false", false)]
    [InlineData("", true)]
    public async Task Generate_AllOfAbstractBase_BaseHasNoAdditionalPropertiesAndDerivedDecides(
        string derivedKeyword, bool expectDerivedAdditionalProperties)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Animal": {
                "type": "object",
                "properties": { "name": { "type": "string" } },
                "x-abstract": true
              },
              "Dog": {
                "type": "object",
                "allOf": [
                  { "$ref": "#/components/schemas/Animal" },
                  { "type": "object", "properties": { "breed": { "type": "string" } } }
                ]{{derivedKeyword}}
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Animal", "Dog");

        // Assert

        HasAdditionalProperties(classes["Animal"]).Should().BeFalse();
        HasAdditionalProperties(classes["Dog"]).Should().Be(expectDerivedAdditionalProperties);
        document.Components!.Schemas!["Animal"].IsAbstract.Should().BeTrue();
    }

    [Theory]
    [InlineData("\"x-abstract\": true")]
    [InlineData("\"additionalProperties\": false")]
    [InlineData("\"unevaluatedProperties\": false")]
    public async Task Generate_ClosedMiddleAndOpenLeaf_LeafReusesRootAdditionalProperties(string middleKeyword)
    {
        // Arrange

        OpenApiDocument document = await LoadThreeLevelAsync(middleKeyword, leafKeyword: null);

        // Act

        var classes = await GenerateAsync(document, "Root", "Middle", "Leaf");

        // Assert

        HasAdditionalProperties(classes["Root"]).Should().BeTrue();
        HasAdditionalProperties(classes["Middle"]).Should().BeFalse();
        HasAdditionalProperties(classes["Leaf"]).Should().BeFalse(
            "the leaf inherits the matching dictionary declared by the root, past the closed middle");
    }

    [Theory]
    [InlineData("\"x-abstract\": true", "\"additionalProperties\": { \"type\": \"string\" }")]
    [InlineData("\"x-abstract\": true", "\"unevaluatedProperties\": { \"type\": \"string\" }")]
    [InlineData("\"additionalProperties\": false", "\"additionalProperties\": { \"type\": \"string\" }")]
    [InlineData("\"additionalProperties\": false", "\"unevaluatedProperties\": { \"type\": \"string\" }")]
    [InlineData("\"unevaluatedProperties\": false", "\"additionalProperties\": { \"type\": \"string\" }")]
    [InlineData("\"unevaluatedProperties\": false", "\"unevaluatedProperties\": { \"type\": \"string\" }")]
    public async Task Generate_ClosedMiddleAndTypedLeaf_LeafShadowsRootAdditionalProperties(string middleKeyword,
        string leafKeyword)
    {
        // Arrange

        OpenApiDocument document = await LoadThreeLevelAsync(middleKeyword, leafKeyword);

        // Act

        var classes = await GenerateAsync(document, "Root", "Middle", "Leaf");

        // Assert

        GetAdditionalPropertiesValueType(classes["Root"]).Should().Be("object");
        HasAdditionalProperties(classes["Middle"]).Should().BeFalse();
        GetAdditionalPropertiesValueType(classes["Leaf"]).Should().Be("string");
        GetAdditionalProperties(classes["Leaf"]).Modifiers.Should().Contain(p => p.IsKind(SyntaxKind.NewKeyword),
            "the leaf hides the root dictionary, which has a different value type");
    }

    [Fact]
    public async Task Generate_EveryLevelOpen_OnlyRootDeclaresAdditionalProperties()
    {
        // Arrange

        OpenApiDocument document = await LoadThreeLevelAsync(middleKeyword: null, leafKeyword: null);

        // Act

        var classes = await GenerateAsync(document, "Root", "Middle", "Leaf");

        // Assert

        HasAdditionalProperties(classes["Root"]).Should().BeTrue();
        HasAdditionalProperties(classes["Middle"]).Should().BeFalse();
        HasAdditionalProperties(classes["Leaf"]).Should().BeFalse();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("\"true\"")]
    [InlineData("1")]
    public async Task Generate_AbstractExtensionNotTrue_AddsAdditionalProperties(string value)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Animal": {
                "type": "object",
                "properties": { "name": { "type": "string" } },
                "x-abstract": {{value}}
              }
            }
            """);

        // Act

        var classes = await GenerateAsync(document, "Animal");

        // Assert

        HasAdditionalProperties(classes["Animal"]).Should().BeTrue();
        document.Components!.Schemas!["Animal"].IsAbstract.Should().BeFalse();
    }

    private static bool HasAdditionalProperties(ClassDeclarationSyntax declaration) =>
        GetAdditionalProperties(declaration) is not null;

    private static PropertyDeclarationSyntax GetAdditionalProperties(ClassDeclarationSyntax declaration) =>
        declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.Identifier.ValueText == AdditionalPropertiesName);

    private static string GetAdditionalPropertiesValueType(ClassDeclarationSyntax declaration) =>
        GetAdditionalProperties(declaration)?.Type
            .DescendantNodesAndSelf()
            .OfType<GenericNameSyntax>()
            .First()
            .TypeArgumentList.Arguments[1]
            .ToString()
            .TrimEnd('?');

    /// <summary>
    /// Loads Root, Middle and Leaf, where each schema extends the previous one with <c>allOf</c>. Root is open.
    /// </summary>
    private static Task<OpenApiDocument> LoadThreeLevelAsync(string middleKeyword, string leafKeyword)
    {
        string middle = middleKeyword is null ? "" : $", {middleKeyword}";
        string leaf = leafKeyword is null ? "" : $", {leafKeyword}";

        return LoadAsync($$"""
            {
              "Root": {
                "type": "object",
                "properties": { "a": { "type": "string" } }
              },
              "Middle": {
                "type": "object",
                "allOf": [ { "$ref": "#/components/schemas/Root" } ],
                "properties": { "b": { "type": "string" } }{{middle}}
              },
              "Leaf": {
                "type": "object",
                "allOf": [ { "$ref": "#/components/schemas/Middle" } ],
                "properties": { "c": { "type": "string" } }{{leaf}}
              }
            }
            """);
    }

    private static async Task<OpenApiDocument> LoadAsync(string schemasJson)
    {
        string json = $$"""
            {
              "openapi": "3.1.1",
              "info": { "title": "Test", "version": "1.0.0" },
              "paths": {},
              "components": { "schemas": {{schemasJson}} }
            }
            """;

        await using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        return await YardarmOpenApiDocument.LoadAsync(stream, TestContext.Current.CancellationToken);
    }

    private static System.IServiceProvider CreateServices(OpenApiDocument document) =>
        new YardarmGenerationSettings().BuildServiceProvider(document);

    private static async Task<Dictionary<string, ClassDeclarationSyntax>> GenerateAsync(OpenApiDocument document,
        params string[] schemaNames)
    {
        var services = CreateServices(document);
        var registry = services.GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

        SyntaxTree[] syntaxTrees = schemaNames
            .Select(name => registry.Get(document.Components!.Schemas![name].CreateRoot(name)).GenerateSyntaxTree()!)
            .ToArray();

        var compilation = CSharpCompilation.Create("Test", syntaxTrees);

        var enricher = new OpenApiCompilationEnricher(
            services.GetRequiredService<IOpenApiElementRegistry>(),
            [new AdditionalPropertiesEnricher(services.GetRequiredService<GenerationContext>())]);

        compilation = await enricher.EnrichAsync(compilation, TestContext.Current.CancellationToken);

        return compilation.SyntaxTrees
            .SelectMany(p => p.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Where(p => p.Parent is not ClassDeclarationSyntax)
            .ToDictionary(p => p.Identifier.ValueText);
    }
}
