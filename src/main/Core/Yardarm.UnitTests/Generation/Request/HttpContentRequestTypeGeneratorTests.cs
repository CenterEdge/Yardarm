using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.Request;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Request;

public class HttpContentRequestTypeGeneratorTests
{
    [Fact]
    public void Generate_UnsupportedRequestMediaType_GeneratesHttpContentRequest()
    {
        // Arrange

        const string documentText = """
            {
              "openapi": "3.0.4",
              "info": {
                "title": "Test",
                "version": "1.0"
              },
              "paths": {
                "/things": {
                  "post": {
                    "operationId": "addThing",
                    "requestBody": {
                      "content": {
                        "application/xml": {
                          "schema": {
                            "type": "string"
                          }
                        }
                      }
                    },
                    "responses": {
                      "204": {
                        "description": "No Content"
                      }
                    }
                  }
                }
              }
            }
            """;

        OpenApiDocument document = OpenApiDocument.Parse(documentText, "json", new OpenApiReaderSettings()).Document;
        var registry = new YardarmGenerationSettings()
            .BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry>();
        var operation = document.Paths.ToLocatedElements().GetOperations().Single();

        // Act

        var declarations = registry.Get(operation).Generate().OfType<ClassDeclarationSyntax>().ToList();

        // Assert

        var httpContentRequest = declarations.Should()
            .ContainSingle(p => p.Identifier.ValueText == "AddThingHttpContentRequest").Subject;
        httpContentRequest.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == BuildContentMethodGenerator.BuildContentMethodName)
            .NormalizeWhitespace().ToFullString().Should()
            .Be("protected override global::System.Net.Http.HttpContent? BuildContent(Yardarm.Sdk.Requests.BuildRequestContext context) => Body;");
    }
}
