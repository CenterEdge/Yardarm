using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation;
using Yardarm.Serialization;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.UnitTests.Generation.Response;

public class ResponseTypeGeneratorTests
{
    private const string ReferencedInlineBodyDocument = """
        {
          "openapi": "3.2.0",
          "info": {
            "title": "Test",
            "version": "1.0"
          },
          "paths": {
            "/things": {
              "get": {
                "operationId": "listThings",
                "responses": {
                  "200": {
                    "$ref": "#/components/responses/Things"
                  }
                }
              }
            }
          },
          "components": {
            "responses": {
              "Things": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        """;

    [Fact]
    public void Generate_ReferencedResponseWithInlineBody_DoesNotGenerateOperationLocalModel()
    {
        // Arrange

        (ITypeGeneratorRegistry registry, ILocatedOpenApiElement<IOpenApiResponse> response, _) = Setup();

        // Act

        var declaration = registry.Get(response).Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        declaration.Identifier.ValueText.Should().Be("ListThingsOkResponse");
        declaration.Members.OfType<ClassDeclarationSyntax>().Should().BeEmpty();
        declaration.Members.OfType<FieldDeclarationSyntax>().Should().BeEmpty();
    }

    [Fact]
    public void Generate_ReferencedResponseWithInlineBody_BodyConstructorUsesComponentModel()
    {
        // Arrange

        (ITypeGeneratorRegistry registry, ILocatedOpenApiElement<IOpenApiResponse> response,
            ILocatedOpenApiElement<IOpenApiResponse> component) = Setup();

        string componentBodyType = GetBodyParameterType(registry.Get(component).Generate()
            .OfType<ClassDeclarationSyntax>().Single());

        // Act

        var declaration = registry.Get(response).Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        componentBodyType.Should().EndWith("ThingsResponse.SchemaModel");
        GetBodyParameterType(declaration).Should().Be(componentBodyType);
    }

    [Fact]
    public void Generate_ComponentResponseWithInlineBody_GeneratesNestedModel()
    {
        // Arrange

        (ITypeGeneratorRegistry registry, _, ILocatedOpenApiElement<IOpenApiResponse> component) = Setup();

        // Act

        var declaration = registry.Get(component).Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        declaration.Identifier.ValueText.Should().Be("ThingsResponse");
        declaration.Members.OfType<ClassDeclarationSyntax>()
            .Should().ContainSingle(p => p.Identifier.ValueText == "SchemaModel");
    }

    [Fact]
    public void GetPrimaryResponse_ReferencedResponse_ReturnsComponentRoot()
    {
        // Arrange

        (_, ILocatedOpenApiElement<IOpenApiResponse> response, ILocatedOpenApiElement<IOpenApiResponse> component) = Setup();

        // Act

        var result = response.GetPrimaryResponse();

        // Assert

        result.IsRoot.Should().BeTrue();
        result.Key.Should().Be("Things");
        result.Element.Should().BeSameAs(component.Element);
    }

    [Fact]
    public void GetPrimaryResponse_InlineResponse_ReturnsResponse()
    {
        // Arrange

        (_, _, ILocatedOpenApiElement<IOpenApiResponse> component) = Setup();

        // Act

        var result = component.GetPrimaryResponse();

        // Assert

        result.Should().BeSameAs(component);
    }

    private const string ChainedReferenceDocument = """
        {
          "openapi": "3.2.0",
          "info": {
            "title": "Test",
            "version": "1.0"
          },
          "paths": {
            "/things": {
              "get": {
                "operationId": "listThings",
                "responses": {
                  "200": {
                    "$ref": "#/components/responses/Alias"
                  }
                }
              }
            }
          },
          "components": {
            "responses": {
              "Alias": {
                "$ref": "#/components/responses/Things"
              },
              "Things": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        """;

    [Fact]
    public void Generate_ChainedResponseReferences_UsesConcreteComponentModel()
    {
        // Arrange

        OpenApiDocument document = OpenApiDocument.Parse(ChainedReferenceDocument, "json", new OpenApiReaderSettings()).Document;
        var registry = new YardarmGenerationSettings()
            .AddExtension<JsonSerializerExtension>()
            .BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry>();

        var operationResponse = document.Paths.ToLocatedElements().GetOperations().Single()
            .GetResponseSet().GetResponses().Single();
        var aliasResponse = document.Components!.Responses!.CreateRoot().Single(p => p.Key == "Alias");

        // Act

        var operationDeclaration = registry.Get(operationResponse).Generate().OfType<ClassDeclarationSyntax>().Single();
        var aliasDeclaration = registry.Get(aliasResponse).Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        GetBodyParameterType(operationDeclaration).Should().EndWith("ThingsResponse.SchemaModel");
        GetBodyParameterType(aliasDeclaration).Should().Be(GetBodyParameterType(operationDeclaration));
        operationDeclaration.Members.OfType<ClassDeclarationSyntax>().Should().BeEmpty();
        aliasDeclaration.Members.OfType<ClassDeclarationSyntax>().Should().BeEmpty();
    }

    [Fact]
    public void GetPrimaryResponse_ChainedReferences_ReturnsConcreteComponentRoot()
    {
        // Arrange

        OpenApiDocument document = OpenApiDocument.Parse(ChainedReferenceDocument, "json", new OpenApiReaderSettings()).Document;
        var operationResponse = document.Paths.ToLocatedElements().GetOperations().Single()
            .GetResponseSet().GetResponses().Single();
        var aliasResponse = document.Components!.Responses!.CreateRoot().Single(p => p.Key == "Alias");

        // Act

        var fromOperation = operationResponse.GetPrimaryResponse();
        var fromAlias = aliasResponse.GetPrimaryResponse();

        // Assert

        fromOperation.Key.Should().Be("Things");
        fromAlias.Key.Should().Be("Things");
        fromOperation.Element.Should().BeSameAs(fromAlias.Element);
    }

    [Fact]
    public void GetPrimaryResponse_CircularReferences_Throws()
    {
        // Arrange

        const string documentText = """
            {
              "openapi": "3.2.0",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {},
              "components": {
                "responses": {
                  "A": { "$ref": "#/components/responses/B" },
                  "B": { "$ref": "#/components/responses/A" }
                }
              }
            }
            """;

        OpenApiDocument document = OpenApiDocument.Parse(documentText, "json", new OpenApiReaderSettings()).Document;
        var response = document.Components!.Responses!.CreateRoot().First();

        // Act

        var act = () => response.GetPrimaryResponse();

        // Assert

        act.Should().Throw<System.InvalidOperationException>().WithMessage("*circular*");
    }

    private static (ITypeGeneratorRegistry Registry, ILocatedOpenApiElement<IOpenApiResponse> Response,
        ILocatedOpenApiElement<IOpenApiResponse> Component) Setup()
    {
        OpenApiDocument document = OpenApiDocument.Parse(ReferencedInlineBodyDocument, "json", new OpenApiReaderSettings()).Document;
        var registry = new YardarmGenerationSettings()
            .AddExtension<JsonSerializerExtension>()
            .BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry>();

        var response = document.Paths.ToLocatedElements().GetOperations().Single()
            .GetResponseSet().GetResponses().Single();
        var component = document.Components!.Responses!.CreateRoot().Single();

        return (registry, response, component);
    }

    private static string GetBodyParameterType(ClassDeclarationSyntax declaration) =>
        declaration.Members.OfType<ConstructorDeclarationSyntax>()
            .SelectMany(p => p.ParameterList.Parameters)
            .Single(p => p.Identifier.ValueText == "body")
            .Type!.NormalizeWhitespace().ToFullString();

    public class JsonSerializerExtension : YardarmExtension
    {
        public override IServiceCollection ConfigureServices(IServiceCollection services) =>
            services.AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/json", 1.0)),
                "Json",
                IdentifierName("JsonTypeSerializer")));
    }
}
