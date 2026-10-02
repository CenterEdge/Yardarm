using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Spec;
using Yardarm.SystemTextJson;

namespace Yardarm.UnitTests.Generation.MediaType;

public class JsonLinesMediaTypeSelectionTests
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
                          "$ref": "#/components/schemas/Thing"
                        }
                      },
                      "application/json": {
                        "schema": {
                          "type": "array",
                          "items": {
                            "$ref": "#/components/schemas/Thing"
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
                    "application/x-ndjson": {
                      "itemSchema": {
                        "$ref": "#/components/schemas/Thing"
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/x-ndjson": {
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
    public void Select_JsonAndJsonLines_SelectsJson()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var selector = serviceProvider.GetRequiredService<IMediaTypeSelector>();

        // Act

        var mediaType = selector.Select(GetResponse(document, "listThings"));

        // Assert

        mediaType!.Key.Should().Be("application/json");
    }

    [Fact]
    public void Select_JsonLinesOnly_SelectsJsonLines()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var selector = serviceProvider.GetRequiredService<IMediaTypeSelector>();
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var mediaType = selector.Select(GetResponse(document, "addThings"));

        // Assert

        mediaType!.Key.Should().Be("application/x-ndjson");
        mediaType.GetBodyType(registry)!.ToString().Should()
            .Be("global::System.Collections.Generic.List<Yardarm.Sdk.Models.Thing>");
    }

    [Fact]
    public void GetTypeName_JsonLinesRequest_UsesJsonLinesNameSegment()
    {
        // Arrange

        var (document, serviceProvider) = CreateDocument();
        var mediaType = GetOperation(document, "addThings").GetRequestBody()!.GetMediaTypes().Single();
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry>();

        // Act

        var typeName = registry.Get(mediaType).TypeInfo.Name;

        // Assert

        typeName.ToString().Should().Be("Yardarm.Sdk.Requests.AddThingsJsonLinesRequest");
    }

    private static (OpenApiDocument document, System.IServiceProvider serviceProvider) CreateDocument()
    {
        OpenApiDocument document = OpenApiDocument.Parse(DocumentText, "json", new OpenApiReaderSettings()).Document;
        var serviceProvider = new YardarmGenerationSettings()
            .AddExtension<SystemTextJsonExtension>()
            .BuildServiceProvider(document);

        return (document, serviceProvider);
    }

    private static ILocatedOpenApiElement<OpenApiOperation> GetOperation(OpenApiDocument document, string operationId) =>
        document.Paths.ToLocatedElements()
            .GetOperations()
            .Single(p => p.Element.OperationId == operationId);

    private static ILocatedOpenApiElement<IOpenApiResponse> GetResponse(OpenApiDocument document, string operationId) =>
        GetOperation(document, operationId)
            .GetResponseSet()
            .GetResponses()
            .Single();
}
