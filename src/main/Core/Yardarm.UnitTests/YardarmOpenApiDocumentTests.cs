using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;

namespace Yardarm.UnitTests;

public class YardarmOpenApiDocumentTests
{
    [Fact]
    public async Task LoadAsync_JsonDocument_ExpectedResult()
    {
        // Arrange

        const string documentText = """
            {
              "openapi": "3.1.1",
              "info": {
                "title": "JSON test",
                "version": "1.0"
              },
              "paths": {}
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

        // Act

        OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
            stream,
            TestContext.Current.CancellationToken);

        // Assert

        document.Info.Title.Should().Be("JSON test");
    }

    [Fact]
    public async Task LoadAsync_YamlDocument_ExpectedResult()
    {
        // Arrange

        const string documentText = """
            openapi: 3.1.1
            info:
              title: YAML test
              version: 1.0
            paths: {}
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(documentText));

        // Act

        OpenApiDocument document = await YardarmOpenApiDocument.LoadAsync(
            stream,
            TestContext.Current.CancellationToken);

        // Assert

        document.Info.Title.Should().Be("YAML test");
    }
}
