using System.Linq;
using System.IO;
using System.Text;
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
        }
    }
}
