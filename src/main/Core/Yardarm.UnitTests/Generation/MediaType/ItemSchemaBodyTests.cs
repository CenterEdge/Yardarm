using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.MediaType;

public class ItemSchemaBodyTests
{
    private const string DocumentText = """
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
                    "description": "OK",
                    "content": {
                      "application/jsonl": {
                        "itemSchema": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "post": {
                "operationId": "addThings",
                "requestBody": {
                  "content": {
                    "application/jsonl": {
                      "itemSchema": {
                        "type": "object",
                        "properties": {
                          "name": {
                            "type": "string"
                          }
                        }
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/jsonl": {
                        "itemSchema": {
                          "$ref": "#/components/schemas/Thing"
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "Thing": {
                "type": "object",
                "properties": {
                  "id": {
                    "type": "integer"
                  }
                }
              }
            }
          }
        }
        """;

    [Fact]
    public void GetBodyType_InlineResponseItemSchema_ListOfItemModel()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var mediaType = GetResponseMediaType(document, System.Net.Http.HttpMethod.Get);
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var bodyType = mediaType.GetBodyType(registry);

        // Assert

        bodyType!.ToString().Should().Be(
            "global::System.Collections.Generic.List<Yardarm.Sdk.Responses.ListThingsOkResponse.ItemSchemaModel>");
    }

    [Fact]
    public void GetBodyType_ReferencedResponseItemSchema_ListOfComponentModel()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var mediaType = GetResponseMediaType(document, System.Net.Http.HttpMethod.Post);
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var bodyType = mediaType.GetBodyType(registry);

        // Assert

        bodyType!.ToString().Should().Be(
            "global::System.Collections.Generic.List<Yardarm.Sdk.Models.Thing>");
    }

    [Fact]
    public void Generate_InlineResponseItemSchema_GeneratesBodyAndItemModel()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var response = GetResponse(document, System.Net.Http.HttpMethod.Get);
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var declaration = registry.Get(response).Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        const string listType =
            "global::System.Collections.Generic.List<Yardarm.Sdk.Responses.ListThingsOkResponse.ItemSchemaModel>";

        declaration.Members.OfType<ClassDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Should().Contain("ItemSchemaModel");
        var getBodyMethod = declaration.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "GetBodyAsync");
        getBodyMethod.ReturnType.ToString().Should().Contain(listType);
        getBodyMethod.Body!.ToString().Should().Contain(
            "TypeSerializerRegistryExtensions.DeserializeListAsync<Yardarm.Sdk.Responses.ListThingsOkResponse.ItemSchemaModel>(");
        declaration.Members.OfType<FieldDeclarationSyntax>()
            .Single(p => p.Declaration.Variables.Single().Identifier.ValueText == "_body")
            .Declaration.Type.ToString().Should().Be(listType + "?");
    }

    [Fact]
    public void Generate_InlineRequestItemSchema_GeneratesBodyAndItemModel()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var mediaType = document.Paths.ToLocatedElements()
            .GetOperations()
            .Single(p => p.Element.OperationId == "addThings")
            .GetRequestBody()!
            .GetMediaTypes()
            .Single();
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var generator = registry.Get(mediaType);
        var declaration = generator.Generate().OfType<ClassDeclarationSyntax>().Single();

        // Assert

        generator.TypeInfo.Name.ToString().Should().Be("Yardarm.Sdk.Requests.AddThingsJsonLinesRequest");
        declaration.Members.OfType<ClassDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Should().Contain("ItemSchemaBody");
        var bodyProperty = declaration.Members.OfType<PropertyDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == RequestMediaTypeGenerator.BodyPropertyName);
        bodyProperty.Type.ToString().Should().Be(
            "global::System.Collections.Generic.List<Yardarm.Sdk.Requests.AddThingsJsonLinesRequest.ItemSchemaBody>");

        // Annotated with the default schema, not the item schema, so enrichers make it nullable without
        // applying item documentation
        var bodySchema = bodyProperty.GetElementAnnotation<IOpenApiSchema>(
            serviceProvider.GetRequiredService<IOpenApiElementRegistry>());
        bodySchema!.Key.Should().Be("schema");
        bodySchema.Parent!.Key.Should().Be("application/jsonl");
        bodySchema.Element.Should().NotBeSameAs(mediaType.Element.ItemSchema);

        declaration.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "BuildContent")
            .Body!.ToString().Should().Contain(
                "TypeSerializerRegistryExtensions.SerializeSequence<Yardarm.Sdk.Requests.AddThingsJsonLinesRequest.ItemSchemaBody>(");
    }

    [Fact]
    public void GetAllSchemas_ItemSchemas_IncludesInlineItemSchemas()
    {
        // Arrange

        var (document, _) = CreateDocument();

        // Act

        var schemas = document.GetAllSchemas().ToList();

        // Assert

        schemas.Where(p => p.Key == "itemSchema").Should().HaveCount(2);
        schemas.Select(p => p.Key).Should().Contain(["id", "name"]);
    }

    private static (OpenApiDocument document, System.IServiceProvider serviceProvider) CreateDocument()
    {
        OpenApiDocument document = OpenApiDocument.Parse(DocumentText, "json", new OpenApiReaderSettings()).Document;
        var serviceProvider = new YardarmGenerationSettings()
            .AddExtension<JsonLinesTestExtension>()
            .BuildServiceProvider(document);

        return (document, serviceProvider);
    }

    private static ILocatedOpenApiElement<IOpenApiResponse> GetResponse(
        OpenApiDocument document,
        System.Net.Http.HttpMethod method) =>
        document.Paths.ToLocatedElements()
            .GetOperations()
            .Single(p => p.Key == method.Method)
            .GetResponseSet()
            .GetResponses()
            .Single();

    private static ILocatedOpenApiElement<IOpenApiMediaType> GetResponseMediaType(
        OpenApiDocument document,
        System.Net.Http.HttpMethod method) =>
        GetResponse(document, method).GetMediaTypes().Single();
}

/// <summary>
/// Registers a JSON Lines serializer so that JSON Lines media types are selectable.
/// </summary>
public sealed class JsonLinesTestExtension : YardarmExtension
{
    public override IServiceCollection ConfigureServices(IServiceCollection services) =>
        services.AddSerializerDescriptor(new SerializerDescriptor(
            ImmutableHashSet.Create(new SerializerMediaType("application/jsonl", 0.8)),
            "JsonLines",
            SyntaxFactory.ParseTypeName("global::Test.JsonLinesTypeSerializer")));
}
