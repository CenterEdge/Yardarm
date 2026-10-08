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
using Yardarm.Generation.Response;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Response;

public class ResponseStreamingTests
{
    private const string DocumentText = """
        {
          "openapi": "3.2.0",
          "info": {
            "title": "Test",
            "version": "1.0"
          },
          "paths": {
            "/lines": {
              "get": {
                "operationId": "listLines",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/jsonl": {
                        "itemSchema": { "$ref": "#/components/schemas/Thing" }
                      }
                    }
                  }
                }
              }
            },
            "/array": {
              "get": {
                "operationId": "listArray",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "array",
                          "items": { "$ref": "#/components/schemas/Thing" }
                        }
                      }
                    }
                  }
                }
              }
            },
            "/streamed-array": {
              "get": {
                "operationId": "listStreamedArray",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        "x-yardarm-streaming": true,
                        "schema": {
                          "type": "array",
                          "items": { "$ref": "#/components/schemas/Thing" }
                        }
                      }
                    }
                  }
                }
              }
            },
            "/not-streamed-array": {
              "get": {
                "operationId": "listNotStreamedArray",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        "x-yardarm-streaming": false,
                        "schema": {
                          "type": "array",
                          "items": { "$ref": "#/components/schemas/Thing" }
                        }
                      }
                    }
                  }
                }
              }
            },
            "/streamed-object": {
              "get": {
                "operationId": "getStreamedObject",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        "x-yardarm-streaming": true,
                        "schema": { "$ref": "#/components/schemas/Thing" }
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
                  "id": { "type": "integer" }
                }
              }
            }
          }
        }
        """;

    private const string ThingType = "Yardarm.Sdk.Models.Thing";

    [Fact]
    public void Resolve_ItemSchema_Streams()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);

        var body = Resolve(document, serviceProvider, "listLines");

        body!.IsStreaming.Should().BeTrue();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.IAsyncEnumerable<{ThingType}>");
        body.ItemType!.ToString().Should().Be(ThingType);
    }

    [Fact]
    public void Resolve_ItemSchemaWithoutStreamingSerializer_ReturnsList()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: false);

        var body = Resolve(document, serviceProvider, "listLines");

        body!.IsStreaming.Should().BeFalse();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.List<{ThingType}>");
        body.ItemType!.ToString().Should().Be(ThingType);
    }

    [Fact]
    public void Resolve_ArrayWithoutExtension_ReturnsList()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);

        var body = Resolve(document, serviceProvider, "listArray");

        body!.IsStreaming.Should().BeFalse();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.List<{ThingType}>");
        body.ItemType.Should().BeNull();
    }

    [Fact]
    public void Resolve_ArrayWithExtension_Streams()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);

        var body = Resolve(document, serviceProvider, "listStreamedArray");

        body!.IsStreaming.Should().BeTrue();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.IAsyncEnumerable<{ThingType}>");
        body.ItemType!.ToString().Should().Be(ThingType);
    }

    [Fact]
    public void Resolve_ArrayWithExtensionFalse_ReturnsList()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);

        var body = Resolve(document, serviceProvider, "listNotStreamedArray");

        body!.IsStreaming.Should().BeFalse();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.List<{ThingType}>");
    }

    [Fact]
    public void Resolve_ArrayWithExtensionWithoutStreamingSerializer_ReturnsList()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: false);

        var body = Resolve(document, serviceProvider, "listStreamedArray");

        body!.IsStreaming.Should().BeFalse();
        body.BodyType.ToString().Should().Be($"global::System.Collections.Generic.List<{ThingType}>");
    }

    [Fact]
    public void Resolve_NonArrayWithExtension_DoesNotStream()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);

        var body = Resolve(document, serviceProvider, "getStreamedObject");

        body!.IsStreaming.Should().BeFalse();
        body.BodyType.ToString().Should().Be(ThingType);
    }

    [Fact]
    public void GenerateResponse_Streaming_GetBodyReturnsAsyncEnumerable()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);
        var response = GetResponse(document, "listStreamedArray");
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        var declaration = registry.Get(response).Generate().OfType<ClassDeclarationSyntax>().Single();

        const string asyncEnumerable = $"global::System.Collections.Generic.IAsyncEnumerable<{ThingType}>";
        var getBodyMethod = declaration.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "GetBodyAsync");
        getBodyMethod.ReturnType.ToString().Should().Contain(asyncEnumerable);
        getBodyMethod.Body!.ToString().Should().Contain(
            $"TypeSerializerRegistryExtensions.DeserializeSequenceAsync<{asyncEnumerable},{ThingType}>(");
        declaration.Members.OfType<FieldDeclarationSyntax>()
            .Single(p => p.Declaration.Variables.Single().Identifier.ValueText == "_body")
            .Declaration.Type.ToString().Should().Be(asyncEnumerable + "?");
    }

    [Fact]
    public void GenerateResponse_ArrayWithoutExtension_GetBodyUsesDeserialize()
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming: true);
        var response = GetResponse(document, "listArray");
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        var declaration = registry.Get(response).Generate().OfType<ClassDeclarationSyntax>().Single();

        declaration.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "GetBodyAsync")
            .Body!.ToString().Should().Contain(
                $"TypeSerializerRegistryExtensions.DeserializeAsync<global::System.Collections.Generic.List<{ThingType}>>(");
    }

    [Theory]
    [InlineData("listLines", true, true)]
    [InlineData("listStreamedArray", true, true)]
    [InlineData("listArray", true, false)]
    [InlineData("listNotStreamedArray", true, false)]
    [InlineData("listLines", false, false)]
    [InlineData("listStreamedArray", false, false)]
    public void GenerateRequest_SetsResponseStreamingOnlyForStreamedBodies(string operationId, bool supportsStreaming,
        bool expected)
    {
        var (document, serviceProvider) = CreateDocument(supportsStreaming);
        var operation = GetOperation(document, operationId);
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        var declaration = registry.Get(operation).Generate().OfType<ClassDeclarationSyntax>().First();

        var constructor = declaration.Members.OfType<ConstructorDeclarationSyntax>().SingleOrDefault();

        if (expected)
        {
            constructor.Should().NotBeNull();
            constructor!.Body!.Statements.Should().ContainSingle().Which.ToString()
                .Should().Be("EnableResponseStreaming=true;");
        }
        else
        {
            constructor.Should().BeNull();
        }
    }

    private static ResponseBodyInfo Resolve(OpenApiDocument document, System.IServiceProvider serviceProvider,
        string operationId)
        => serviceProvider.GetRequiredService<IResponseBodyResolver>().Resolve(GetResponse(document, operationId));

    private static (OpenApiDocument document, System.IServiceProvider serviceProvider) CreateDocument(
        bool supportsStreaming)
    {
        OpenApiDocument document = OpenApiDocument.Parse(DocumentText, "json", new OpenApiReaderSettings()).Document;
        var settings = new YardarmGenerationSettings();
        var serviceProvider = (supportsStreaming
                ? settings.AddExtension<StreamingJsonTestExtension>()
                : settings.AddExtension<NonStreamingJsonTestExtension>())
            .BuildServiceProvider(document);

        return (document, serviceProvider);
    }

    private static ILocatedOpenApiElement<OpenApiOperation> GetOperation(OpenApiDocument document, string operationId)
        => document.Paths.ToLocatedElements()
            .GetOperations()
            .Single(p => p.Element.OperationId == operationId);

    private static ILocatedOpenApiElement<IOpenApiResponse> GetResponse(OpenApiDocument document, string operationId)
        => GetOperation(document, operationId)
            .GetResponseSet()
            .GetResponses()
            .Single();
}

/// <summary>
/// Registers JSON and JSON Lines serializers which support streaming.
/// </summary>
public sealed class StreamingJsonTestExtension : YardarmExtension
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
        => TestSerializers.Add(services, supportsStreaming: true);
}

/// <summary>
/// Registers JSON and JSON Lines serializers which do not support streaming.
/// </summary>
public sealed class NonStreamingJsonTestExtension : YardarmExtension
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
        => TestSerializers.Add(services, supportsStreaming: false);
}

internal static class TestSerializers
{
    public static IServiceCollection Add(IServiceCollection services, bool supportsStreaming)
        => services
            .AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/json", 1.0)),
                "Json",
                SyntaxFactory.ParseTypeName("global::Test.JsonTypeSerializer"),
                supportsStreaming))
            .AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/jsonl", 0.8)),
                "JsonLines",
                SyntaxFactory.ParseTypeName("global::Test.JsonLinesTypeSerializer"),
                supportsStreaming));
}
