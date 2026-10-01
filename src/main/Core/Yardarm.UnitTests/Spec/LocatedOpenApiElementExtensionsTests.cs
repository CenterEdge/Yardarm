using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Spec
{
    public class LocatedOpenApiElementExtensionsTests
    {
        [Fact]
        public void GetOperations_OpenApi32ExtendedMethods_ExpectedResult()
        {
            // Arrange

            const string documentText = """
                {
                  "openapi": "3.2.0",
                  "info": {
                    "title": "Test",
                    "version": "1.0"
                  },
                  "paths": {
                    "/things": {
                      "query": {
                        "operationId": "queryThings",
                        "responses": {
                          "200": {
                            "description": "OK"
                          }
                        }
                      },
                      "additionalOperations": {
                        "LINK": {
                          "operationId": "linkThings",
                          "responses": {
                            "200": {
                              "description": "OK"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;

            OpenApiDocument document = OpenApiDocument.Parse(documentText, "json", new OpenApiReaderSettings()).Document;

            // Act

            var operations = document.Paths.ToLocatedElements().GetOperations().ToArray();

            // Assert

            operations.Select(p => (p.Key, p.Element.OperationId)).Should().Equal(
                ("QUERY", "queryThings"),
                ("LINK", "linkThings"));
        }

        [Theory]
        [InlineData("3.0.4")]
        [InlineData("3.1.1")]
        public async Task LoadAsync_ItemSchemaExtension_ExpectedResult(string specificationVersion)
        {
            // Arrange

            string documentText = $$"""
                {
                  "openapi": "{{specificationVersion}}",
                  "info": {
                    "title": "Test",
                    "version": "1.0"
                  },
                  "paths": {
                    "/things": {
                      "get": {
                        "responses": {
                          "200": {
                            "description": "OK",
                            "content": {
                              "application/jsonl": {
                                "x-oai-itemSchema": {
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
                      }
                    }
                  },
                  "components": {
                    "responses": {
                      "Things": {
                        "description": "OK",
                        "content": {
                          "application/jsonl": {
                            "x-oai-itemSchema": {
                              "type": "string"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

            // Act

            OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
                stream,
                TestContext.Current.CancellationToken);
            IOpenApiResponse response = document.Paths["/things"].Operations[HttpMethod.Get].Responses["200"];
            IOpenApiMediaType jsonLinesMediaType = response.Content["application/jsonl"];
            IOpenApiMediaType componentJsonLinesMediaType =
                document.Components!.Responses["Things"].Content["application/jsonl"];

            // Assert

            jsonLinesMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Properties.Should().ContainKey("id");
            jsonLinesMediaType.Extensions?.Should().NotContainKey("x-oai-itemSchema");
            componentJsonLinesMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.String);
        }

        [Fact]
        public void OpenApiItemSchemaConverter_ConvertsAllResponseContent()
        {
            // Arrange

            var responseJsonLinesMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "string" })
            };
            var responseJsonMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "integer" })
            };
            var requestJsonLinesMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "boolean" })
            };
            var document = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/things"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new OpenApiOperation
                            {
                                RequestBody = new OpenApiRequestBody
                                {
                                    Content = new Dictionary<string, IOpenApiMediaType>
                                    {
                                        ["application/jsonl"] = requestJsonLinesMediaType
                                    }
                                },
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "OK",
                                        Content = new Dictionary<string, IOpenApiMediaType>
                                        {
                                            ["application/jsonl"] = responseJsonLinesMediaType,
                                            ["application/json"] = responseJsonMediaType
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act

            OpenApiItemSchemaConverter.Convert(document, OpenApiSpecVersion.OpenApi3_1);

            // Assert

            responseJsonLinesMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.String);
            responseJsonLinesMediaType.Extensions.Should().NotContainKey("x-oai-itemSchema");
            responseJsonMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.Integer);
            responseJsonMediaType.Extensions.Should().NotContainKey("x-oai-itemSchema");
            requestJsonLinesMediaType.ItemSchema.Should().BeNull();
            requestJsonLinesMediaType.Extensions.Should().ContainKey("x-oai-itemSchema");
        }

        [Fact]
        public void OpenApiItemSchemaConverter_ConvertsNestedReusableAndCyclicPathItemResponses()
        {
            // Arrange

            var callbackMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "string" })
            };
            var webhookMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "integer" })
            };
            var componentPathItemMediaType = new OpenApiMediaType
            {
                Extensions = CreateItemSchemaExtension(new JsonObject { ["type"] = "boolean" })
            };
            var callbackPathItem = CreatePathItem(callbackMediaType);
            var document = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/things"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new OpenApiOperation()
                        }
                    }
                },
                Webhooks = new Dictionary<string, IOpenApiPathItem>
                {
                    ["thingChanged"] = CreatePathItem(webhookMediaType)
                },
                Components = new OpenApiComponents
                {
                    PathItems = new Dictionary<string, IOpenApiPathItem>
                    {
                        ["ThingEvents"] = CreatePathItem(componentPathItemMediaType)
                    }
                }
            };
            var callback = new OpenApiCallback
            {
                PathItems = []
            };
            callback.PathItems.Add(
                RuntimeExpression.Build("$request.body#/callbackUrl"),
                callbackPathItem);
            document.Paths["/things"].Operations[HttpMethod.Get].Callbacks =
                new Dictionary<string, IOpenApiCallback> { ["onThingChanged"] = callback };
            callbackPathItem.Operations[HttpMethod.Post].Callbacks =
                new Dictionary<string, IOpenApiCallback> { ["onThingChanged"] = callback };

            // Act

            OpenApiItemSchemaConverter.Convert(document, OpenApiSpecVersion.OpenApi3_1);

            // Assert

            callbackMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.String);
            webhookMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.Integer);
            componentPathItemMediaType.ItemSchema.Should().BeOfType<OpenApiSchema>()
                .Which.Type.Should().Be(JsonSchemaType.Boolean);
        }

        private static OpenApiPathItem CreatePathItem(OpenApiMediaType mediaType) =>
            new()
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Post] = new OpenApiOperation
                    {
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK",
                                Content = new Dictionary<string, IOpenApiMediaType>
                                {
                                    ["application/jsonl"] = mediaType
                                }
                            }
                        }
                    }
                }
            };

        private static Dictionary<string, IOpenApiExtension> CreateItemSchemaExtension(JsonNode node) =>
            new()
            {
                ["x-oai-itemSchema"] = new JsonNodeExtension(node)
            };

        [Theory]
        [InlineData("3.0.4")]
        [InlineData("3.1.1")]
        public async Task LoadAsync_AdditionalOperationsExtension_ExpectedResult(string specificationVersion)
        {
            // Arrange

            string documentText = $$"""
                {
                  "openapi": "{{specificationVersion}}",
                  "info": {
                    "title": "Test",
                    "version": "1.0"
                  },
                  "paths": {
                    "/things": {
                      "x-oai-additionalOperations": {
                        "QUERY": {
                          "operationId": "queryThings",
                          "responses": {
                            "200": {
                              "description": "OK"
                            }
                          }
                        },
                        "LINK": {
                          "operationId": "linkThings",
                          "responses": {
                            "200": {
                              "description": "OK"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;
            var settings = new OpenApiReaderSettings();
            var result = OpenApiDocument.Parse(documentText, "json", settings);

            // Act

            result.Document.Paths.ToLocatedElements().GetOperations().Should().BeEmpty();
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));
            OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
                stream,
                TestContext.Current.CancellationToken);
            var operations = document.Paths.ToLocatedElements().GetOperations().ToArray();

            // Assert

            operations.Select(p => (p.Key, p.Element.OperationId)).Should().Equal(
                ("QUERY", "queryThings"),
                ("LINK", "linkThings"));

            await using var serializedStream = new MemoryStream();
            await document.SerializeAsJsonAsync(
                serializedStream,
                specificationVersion.StartsWith("3.0", StringComparison.Ordinal)
                    ? OpenApiSpecVersion.OpenApi3_0
                    : OpenApiSpecVersion.OpenApi3_1,
                TestContext.Current.CancellationToken);

            var reader = new Utf8JsonReader(serializedStream.ToArray());
            var additionalOperationsPropertyCount = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName &&
                    reader.GetString() == "x-oai-additionalOperations")
                {
                    additionalOperationsPropertyCount++;
                }
            }

            additionalOperationsPropertyCount.Should().Be(1);

            await using var roundTripStream = new MemoryStream(serializedStream.ToArray());
            OpenApiDocument roundTripDocument = await YardarmOpenApiDocument.LoadAsync(
                roundTripStream,
                TestContext.Current.CancellationToken);

            roundTripDocument.Paths.ToLocatedElements().GetOperations()
                .Select(p => (p.Key, p.Element.OperationId)).Should().Equal(
                    ("QUERY", "queryThings"),
                    ("LINK", "linkThings"));
        }

        [Theory]
        [InlineData("3.0.4")]
        [InlineData("3.1.1")]
        public async Task LoadAsync_AdditionalOperationsExtension_WithPathParameter_ExpectedResult(
            string specificationVersion)
        {
            // Arrange

            string documentText = $$"""
                    {
                      "openapi": "{{specificationVersion}}",
                      "info": {
                        "title": "Test",
                        "version": "1.0"
                      },
                      "paths": {
                        "/org/{businessEntityId}/pricelists/default/entries": {
                          "x-oai-additionalOperations": {
                            "QUERY": {
                              "operationId": "getEntriesWithPrices",
                              "parameters": [
                                {
                                  "name": "businessEntityId",
                                  "in": "path",
                                  "required": true,
                                  "schema": {
                                    "type": "integer",
                                    "format": "int64"
                                  }
                                }
                              ],
                              "responses": {
                                "200": {
                                  "description": "OK"
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                    """;
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

            // Act

            OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
                    stream,
                    TestContext.Current.CancellationToken);
            var operation = document.Paths.ToLocatedElements().GetOperations().Single();

            // Assert

            operation.Key.Should().Be("QUERY");
            operation.Element.OperationId.Should().Be("getEntriesWithPrices");
            operation.GetAllParameters().Select(p => p.Element.Name).Should().Equal("businessEntityId");
        }

        [Fact]
        public async Task LoadAsync_AdditionalOperationsExtension_WithMismatchedPathParameter_Throws()
        {
            // Arrange

            const string documentText = """
                {
                  "openapi": "3.1.1",
                  "info": {
                    "title": "Test",
                    "version": "1.0"
                  },
                  "paths": {
                    "/org/{businessEntityId}": {
                      "x-oai-additionalOperations": {
                        "QUERY": {
                          "operationId": "getEntriesWithPrices",
                          "parameters": [
                            {
                              "name": "organizationId",
                              "in": "path",
                              "required": true,
                              "schema": {
                                "type": "integer",
                                "format": "int64"
                              }
                            }
                          ],
                          "responses": {
                            "200": {
                              "description": "OK"
                            }
                          }
                        }
                      }
                    }
                  }
                }
                """;
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

            // Act

            Func<Task> act = () => YardarmOpenApiDocument.LoadAsync(
                stream,
                TestContext.Current.CancellationToken);

            // Assert

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*organizationId*");
        }

        [Fact]
        public async Task LoadAsync_AdditionalOperationsExtension_WithoutResponses_Throws()
        {
            // Arrange

            const string documentText = """
                {
                  "openapi": "3.1.1",
                  "info": {
                    "title": "Test",
                    "version": "1.0"
                  },
                  "paths": {
                    "/things": {
                      "x-oai-additionalOperations": {
                        "QUERY": {
                          "operationId": "queryThings"
                        }
                      }
                    }
                  }
                }
                """;
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

            // Act

            Func<Task> act = () => YardarmOpenApiDocument.LoadAsync(
                stream,
                TestContext.Current.CancellationToken);

            // Assert

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*Responses*");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task LoadAsync_AdditionalOperationsExtension_OnReferencedPathItem_ExpectedResult(
            bool chainedReference)
        {
            // Arrange

            string pathItem = chainedReference
                    ? """
                        "Things": {
                          "$ref": "#/components/pathItems/AdditionalOperations"
                        },
                        """
                    : string.Empty;
            string pathReference = chainedReference ? "Things" : "AdditionalOperations";
            string documentText = $$"""
                    {
                      "openapi": "3.1.1",
                      "info": {
                        "title": "Test",
                        "version": "1.0"
                      },
                      "paths": {
                        "/things": {
                          "$ref": "#/components/pathItems/{{pathReference}}"
                        }
                      },
                      "components": {
                        "pathItems": {
                          {{pathItem}}
                          "AdditionalOperations": {
                            "x-oai-additionalOperations": {
                              "QUERY": {
                                "operationId": "queryThings",
                                "responses": {
                                  "200": {
                                    "description": "OK"
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                    """;

            // Act

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));
            OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
                    stream,
                    TestContext.Current.CancellationToken);

            // Assert

            document.Paths.ToLocatedElements().GetOperations()
                    .Select(p => (p.Key, p.Element.OperationId)).Should().Equal(("QUERY", "queryThings"));
        }
    }
}
