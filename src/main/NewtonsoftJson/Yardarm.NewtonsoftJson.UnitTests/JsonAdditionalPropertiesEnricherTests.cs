using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable xUnit1051

namespace Yardarm.NewtonsoftJson.UnitTests;

public class JsonAdditionalPropertiesEnricherTests
{
    [Fact]
    public async Task EnrichAsync_SchemaNestedInResponse_AddsJsonExtensionData()
    {
        // Arrange

        // Compilation units are annotated with the element of their root type, here the response,
        // while the inline body schema is only annotated on the nested class.
        ILocatedOpenApiElement<IOpenApiResponse> response = new OpenApiResponse
        {
            Content = new Dictionary<string, IOpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.Object } }
            }
        }.CreateRoot("200");
        var schema = response.GetMediaTypes().Single().GetSchema()!;

        var elementRegistry = new TestElementRegistry();
        var syntaxTree = CreateSyntaxTree(response, schema, elementRegistry);
        var compilation = CSharpCompilation.Create("Test", [syntaxTree]);

        var enricher = new OpenApiCompilationEnricher(elementRegistry,
            [new JsonAdditionalPropertiesEnricher(new TestJsonSerializationNamespace())]);

        // Act

        var result = await enricher.EnrichAsync(compilation, TestContext.Current.CancellationToken);

        // Assert

        var schemaClass = result.SyntaxTrees.Single().GetRoot()
            .DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "SchemaModel");

        schemaClass.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(p => p.AttributeLists).SelectMany(p => p.Attributes)
            .Should().ContainSingle(p => p.Name.ToString().EndsWith("JsonExtensionDataAttribute"));

        var property = schemaClass.Members.OfType<PropertyDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "AdditionalProperties");
        property.ExpressionBody.Should().NotBeNull();
        property.AttributeLists.SelectMany(p => p.Attributes)
            .Should().ContainSingle(p => p.Name.ToString().EndsWith("JsonIgnoreAttribute"));
    }

    private static SyntaxTree CreateSyntaxTree(ILocatedOpenApiElement<IOpenApiResponse> response,
        ILocatedOpenApiElement<IOpenApiSchema> schema, IOpenApiElementRegistry elementRegistry)
    {
        var compilationUnit = ParseCompilationUnit("""
            public class GetThingOkResponse
            {
                public class SchemaModel
                {
                    public System.Collections.Generic.IDictionary<string, dynamic?> AdditionalProperties { get; } =
                        new System.Collections.Generic.Dictionary<string, dynamic?>();
                }
            }
            """);

        var additionalProperties = compilationUnit.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();
        compilationUnit = compilationUnit.ReplaceNode(additionalProperties,
            additionalProperties.AddSpecialMemberAnnotation(SpecialMembers.AdditionalProperties));

        var schemaClass = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "SchemaModel");
        compilationUnit = compilationUnit.ReplaceNode(schemaClass,
            schemaClass.AddElementAnnotation(schema, elementRegistry));

        return CSharpSyntaxTree.Create(compilationUnit.AddElementAnnotation(response, elementRegistry));
    }

    private sealed class TestJsonSerializationNamespace : IJsonSerializationNamespace
    {
        public NameSyntax Name { get; } = IdentifierName("Json");
        public NameSyntax DiscriminatorConverter { get; } = IdentifierName("DiscriminatorConverter");
        public NameSyntax DynamicAdditionalPropertiesDictionary { get; } = IdentifierName("DynamicAdditionalPropertiesDictionary");
        public NameSyntax JsonTypeSerializer { get; } = IdentifierName("JsonTypeSerializer");
        public NameSyntax JsonLinesTypeSerializer { get; } = IdentifierName("JsonLinesTypeSerializer");
        public NameSyntax NullableDynamicAdditionalPropertiesDictionary { get; } = IdentifierName("NullableDynamicAdditionalPropertiesDictionary");
        public NameSyntax OpenApiDateConverter { get; } = IdentifierName("OpenApiDateConverter");

        public TypeSyntax AdditionalPropertiesDictionary(TypeSyntax valueType) =>
            GenericName(Identifier("AdditionalPropertiesDictionary"), TypeArgumentList(SingletonSeparatedList(valueType)));

        public TypeSyntax JsonExtensibleEnumConverterName(TypeSyntax enumType) => throw new NotSupportedException();

        public TypeSyntax JsonExternallyDiscriminatedUnionConverterName(TypeSyntax schemaType) => throw new NotSupportedException();
    }

    private sealed class TestElementRegistry : IOpenApiElementRegistry
    {
        private readonly Dictionary<string, ILocatedOpenApiElement> _elements = [];

        public ILocatedOpenApiElement<T> Get<T>(string key) where T : IOpenApiElement =>
            TryGet<T>(key, out var element) ? element : throw new KeyNotFoundException();

        public bool TryGet<T>(string key, [MaybeNullWhen(false)] out ILocatedOpenApiElement<T> element) where T : IOpenApiElement
        {
            if (_elements.TryGetValue(key, out var untypedElement) && untypedElement is ILocatedOpenApiElement<T> typedElement)
            {
                element = typedElement;
                return true;
            }

            element = null!;
            return false;
        }

        public string Add<T>(ILocatedOpenApiElement<T> element) where T : IOpenApiElement
        {
            string key = Guid.NewGuid().ToString();
            _elements.Add(key, element);
            return key;
        }
    }
}
