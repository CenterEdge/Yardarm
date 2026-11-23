using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Schema;

public class DictionarySchemaTests
{
    private const string InlineObject = """{ "type": "object", "properties": { "x": { "type": "string" } } }""";
    private const string InlineEnum = """{ "type": "string", "enum": [ "a", "b" ] }""";

    public static TheoryData<string, string, string> InlineValueSchemas => new()
    {
        { "additionalProperties", InlineObject, nameof(ClassDeclarationSyntax) },
        { "additionalProperties", InlineEnum, nameof(EnumDeclarationSyntax) },
        { "unevaluatedProperties", InlineObject, nameof(ClassDeclarationSyntax) },
        { "unevaluatedProperties", InlineEnum, nameof(EnumDeclarationSyntax) },
    };

    [Theory]
    [MemberData(nameof(InlineValueSchemas))]
    public async Task Get_ReferencedDictionaryWithInlineValue_NamesValueInSchemaNamespace(string keyword,
        string valueSchema, string expectedDeclarationKind)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Map": { "type": "object", "{{keyword}}": {{valueSchema}} },
              "Holder": {
                "type": "object",
                "properties": { "map": { "$ref": "#/components/schemas/Map" } }
              }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        var mapGenerator = registry.Get(document.Components!.Schemas!["Map"].CreateRoot("Map"));
        SyntaxTree mapTree = mapGenerator.GenerateSyntaxTree();
        SyntaxTree holderTree = registry.Get(document.Components!.Schemas!["Holder"].CreateRoot("Holder"))
            .GenerateSyntaxTree();

        // Assert

        string valueTypeName = GetDictionaryValueType(mapGenerator.TypeInfo.Name);
        valueTypeName.Should().EndWith(".MapValue");

        mapTree.Should().NotBeNull();
        var declaration = mapTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Should()
            .ContainSingle().Subject;
        declaration.Identifier.ValueText.Should().Be("MapValue");
        declaration.GetType().Name.Should().Be(expectedDeclarationKind);

        holderTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<BaseTypeDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Should().Equal(["Holder"], "the referenced dictionary's value type is generated with the dictionary");
        GetPropertyType(holderTree, "Map").Should().Be(mapGenerator.TypeInfo.Name.ToString());
    }

    [Theory]
    [MemberData(nameof(InlineValueSchemas))]
    public async Task Generate_InlineDictionaryWithInlineValue_NestsValueInOwner(string keyword,
        string valueSchema, string expectedDeclarationKind)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Holder": {
                "type": "object",
                "properties": {
                  "map": { "type": "object", "{{keyword}}": {{valueSchema}} }
                }
              }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        SyntaxTree holderTree = registry.Get(document.Components!.Schemas!["Holder"].CreateRoot("Holder"))
            .GenerateSyntaxTree();

        // Assert

        holderTree.Should().NotBeNull();
        var holder = holderTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "Holder");

        var declaration = holder.Members.OfType<BaseTypeDeclarationSyntax>().Should().ContainSingle().Subject;
        declaration.Identifier.ValueText.Should().Be("MapModelValue");
        declaration.GetType().Name.Should().Be(expectedDeclarationKind);

        GetDictionaryValueType(ParseType(GetPropertyType(holderTree, "Map"))).Should().EndWith(".Holder.MapModelValue");
    }

    [Fact]
    public async Task Generate_DictionaryWithReferencedValue_DoesNotEmitValue()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Item": { "type": "object", "properties": { "x": { "type": "string" } } },
              "Map": { "type": "object", "additionalProperties": { "$ref": "#/components/schemas/Item" } }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        var mapGenerator = registry.Get(document.Components!.Schemas!["Map"].CreateRoot("Map"));

        // Assert

        GetDictionaryValueType(mapGenerator.TypeInfo.Name).Should().EndWith(".Item");
        mapGenerator.GenerateSyntaxTree().Should().BeNull();
    }

    [Fact]
    public async Task Generate_DictionaryWithPrimitiveValue_DoesNotEmitValue()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("""
            {
              "Map": { "type": "object", "additionalProperties": { "type": "string" } }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        var mapGenerator = registry.Get(document.Components!.Schemas!["Map"].CreateRoot("Map"));

        // Assert

        mapGenerator.TypeInfo.Name.ToString().Should()
            .Be("global::System.Collections.Generic.Dictionary<string,string>");
        mapGenerator.GenerateSyntaxTree().Should().BeNull();
    }

    [Fact]
    public async Task Generate_NestedDictionaryWithInlineValue_NamesValueUnderOuterValue()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Map": {
                "type": "object",
                "additionalProperties": { "type": "object", "additionalProperties": {{InlineObject}} }
              }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        var mapGenerator = registry.Get(document.Components!.Schemas!["Map"].CreateRoot("Map"));
        SyntaxTree mapTree = mapGenerator.GenerateSyntaxTree();

        // Assert

        GetDictionaryValueType(ParseType(GetDictionaryValueType(mapGenerator.TypeInfo.Name)))
            .Should().EndWith(".MapValueValue");
        mapTree.Should().NotBeNull();
        mapTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Should().Equal("MapValueValue");
    }

    [Theory]
    [InlineData("""{ "type": ["string", "null"], "enum": ["a", "b", null] }""")]
    [InlineData("""{ "oneOf": [ { "type": "string", "enum": ["a", "b"] }, { "type": "null" } ] }""")]
    public async Task Get_DictionaryWithNullableInlineEnumValue_ValueTypeIsNullable(string valueSchema)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Map": { "type": "object", "additionalProperties": {{valueSchema}} }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        var mapGenerator = registry.Get(document.Components!.Schemas!["Map"].CreateRoot("Map"));

        // Assert

        GetDictionaryValueType(mapGenerator.TypeInfo.Name).Should().EndWith(".MapValue?");
    }

    [Theory]
    [InlineData("additionalProperties")]
    [InlineData("unevaluatedProperties")]
    public async Task Generate_ObjectWithDictionaryOfInlineValues_EmitsValueType(string keyword)
    {
        // Arrange

        OpenApiDocument document = await LoadAsync($$"""
            {
              "Widget": {
                "type": "object",
                "properties": { "name": { "type": "string" } },
                "{{keyword}}": { "type": "object", "additionalProperties": {{InlineObject}} }
              }
            }
            """);
        var registry = CreateRegistry(document);

        // Act

        SyntaxTree widgetTree = registry.Get(document.Components!.Schemas!["Widget"].CreateRoot("Widget"))
            .GenerateSyntaxTree();

        // Assert

        widgetTree.Should().NotBeNull();
        var widget = widgetTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "Widget");
        widget.Members.OfType<ClassDeclarationSyntax>().Select(p => p.Identifier.ValueText)
            .Should().Equal($"{keyword[0].ToString().ToUpperInvariant()}{keyword[1..]}ModelValue");
    }

    [Fact]
    public async Task Generate_HeaderWithDictionaryOfInlineValues_EmitsValueTypeInHeaderNamespace()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync("{}", $$"""
            {
              "X-Map": { "schema": { "type": "object", "additionalProperties": {{InlineObject}} } }
            }
            """);
        var headerRegistry = new YardarmGenerationSettings().BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry<IOpenApiHeader>>();

        // Act

        var headerGenerator = headerRegistry.Get(
            LocatedOpenApiElement.CreateRoot(document.Components!.Headers!["X-Map"], "X-Map"));
        SyntaxTree headerTree = headerGenerator.GenerateSyntaxTree();

        // Assert

        headerTree.Should().NotBeNull();
        headerTree!.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Should().Equal("XMapValue");
        GetDictionaryValueType(headerGenerator.TypeInfo.Name).Should().EndWith(".XMapValue");
    }

    [Fact]
    public async Task Generate_HeaderReferencingDictionaryOfInlineValues_DoesNotEmitValueAgain()
    {
        // Arrange

        OpenApiDocument document = await LoadAsync(
            $$"""{ "Map": { "type": "object", "additionalProperties": {{InlineObject}} } }""",
            """{ "X-Map": { "schema": { "$ref": "#/components/schemas/Map" } } }""");
        var headerRegistry = new YardarmGenerationSettings().BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry<IOpenApiHeader>>();

        // Act

        var headerGenerator = headerRegistry.Get(
            LocatedOpenApiElement.CreateRoot(document.Components!.Headers!["X-Map"], "X-Map"));

        // Assert

        headerGenerator.GenerateSyntaxTree().Should().BeNull("the referenced schema emits its value type");
        GetDictionaryValueType(headerGenerator.TypeInfo.Name).Should().EndWith(".MapValue");
    }

    [Theory]
    [InlineData("additionalProperties", true)]
    [InlineData("unevaluatedProperties", true)]
    [InlineData("unevaluatedProperties", false)]
    public async Task GetAllSchemas_DictionaryWithInlineValue_IncludesValueOnlyWhenOpen(string keyword, bool open)
    {
        // Arrange

        string closed = open ? "" : """, "additionalProperties": false""";
        OpenApiDocument document = await LoadAsync($$"""
            {
              "Map": { "type": "object", "{{keyword}}": {{InlineObject}}{{closed}} }
            }
            """);

        // Act

        var schemas = document.GetAllSchemas().Select(p => p.ToString()).ToList();

        // Assert

        if (open)
        {
            schemas.Should().Contain($"Map:IOpenApiSchema/{keyword}:IOpenApiSchema");
        }
        else
        {
            schemas.Should().NotContain(p => p.Contains(keyword));
        }
    }

    private static TypeSyntax ParseType(string typeName) =>
        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(typeName);

    private static string GetDictionaryValueType(TypeSyntax dictionaryType) =>
        dictionaryType.DescendantNodesAndSelf()
            .OfType<GenericNameSyntax>()
            .First(p => p.Identifier.ValueText == "Dictionary")
            .TypeArgumentList.Arguments[1]
            .ToString();

    private static string GetPropertyType(SyntaxTree tree, string propertyName) =>
        tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == propertyName)
            .Type.ToString();

    private static ITypeGeneratorRegistry<IOpenApiSchema> CreateRegistry(OpenApiDocument document) =>
        new YardarmGenerationSettings().BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

    private static async Task<OpenApiDocument> LoadAsync(string schemasJson, string headersJson = "{}")
    {
        string json = $$"""
            {
              "openapi": "3.1.1",
              "info": { "title": "Test", "version": "1.0.0" },
              "paths": {},
              "components": { "schemas": {{schemasJson}}, "headers": {{headersJson}} }
            }
            """;

        await using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        return await YardarmOpenApiDocument.LoadAsync(stream, TestContext.Current.CancellationToken);
    }
}
