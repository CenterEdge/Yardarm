using System.Linq;
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
    }
}
