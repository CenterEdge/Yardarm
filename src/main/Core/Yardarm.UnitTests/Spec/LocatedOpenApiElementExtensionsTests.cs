using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
