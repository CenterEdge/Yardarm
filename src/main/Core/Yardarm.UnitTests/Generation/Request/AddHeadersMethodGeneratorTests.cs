using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Request;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Request;

public class AddHeadersMethodGeneratorTests
{
    private const string Accept = "requestMessage.Headers.Accept.Add(new global::System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(";

    private static readonly Dictionary<string, double> Qualities = new()
    {
        ["application/json"] = 1.0,
        ["application/jsonl"] = 0.95,
        ["text/json"] = 0.9,
        ["application/*+json"] = 0,
    };

    [Fact]
    public void Generate_SingleMediaType_SingleAccept()
    {
        string[] result = GenerateAccept("""
            "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "string" } } } }
            """);

        result.Should().Equal($"{Accept}\"application/json\", 1));");
    }

    [Fact]
    public void Generate_MultipleMediaTypes_OrderedByQualityExcludingUnsupported()
    {
        string[] result = GenerateAccept("""
            "200": {
              "description": "OK",
              "content": {
                "text/json": { "schema": { "type": "string" } },
                "application/xml": { "schema": { "type": "string" } },
                "application/*+json": { "schema": { "type": "string" } },
                "application/json": { "schema": { "type": "string" } },
                "application/jsonl": { "schema": { "type": "string" } }
              }
            }
            """);

        result.Should().Equal(
            $"{Accept}\"application/json\", 1));",
            $"{Accept}\"application/jsonl\", 0.95));",
            $"{Accept}\"text/json\", 0.9));");
    }

    [Fact]
    public void Generate_MediaTypeAcrossResponses_Deduplicated()
    {
        string[] result = GenerateAccept("""
            "200": { "description": "OK", "content": { "text/json": { "schema": { "type": "string" } } } },
            "400": { "description": "Bad", "content": { "text/json": { "schema": { "type": "string" } }, "application/json": { "schema": { "type": "string" } } } }
            """);

        result.Should().Equal(
            $"{Accept}\"application/json\", 1));",
            $"{Accept}\"text/json\", 0.9));");
    }

    [Fact]
    public void Generate_BinaryOnly_FallsBackToSelectorWithoutQuality()
    {
        string[] result = GenerateAccept("""
            "200": { "description": "OK", "content": { "application/octet-stream": { "schema": { "type": "string", "format": "binary" } } } }
            """);

        result.Should().Equal($"{Accept}\"application/octet-stream\"));");
    }

    [Fact]
    public void Generate_NoContent_NoAccept()
    {
        string[] result = GenerateAccept("""
            "204": { "description": "No Content" }
            """);

        result.Should().BeEmpty();
    }

    private static string[] GenerateAccept(string responses)
    {
        string documentText = $$"""
            {
              "openapi": "3.0.4",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {
                "/things": {
                  "get": {
                    "operationId": "getThings",
                    "responses": { {{responses}} }
                  }
                }
              }
            }
            """;

        OpenApiDocument document = OpenApiDocument.Parse(documentText, "json", new OpenApiReaderSettings()).Document;
        IServiceProvider serviceProvider = new YardarmGenerationSettings().BuildServiceProvider(document);
        var operation = document.Paths.ToLocatedElements().GetOperations().Single();

        var generator = new AddHeadersMethodGenerator(
            serviceProvider.GetRequiredService<IRequestsNamespace>(),
            serviceProvider.GetRequiredService<IMediaTypeSelector>(),
            new StubSerializerSelector(),
            serviceProvider.GetRequiredService<INameFormatterSelector>(),
            serviceProvider.GetRequiredService<ISerializationNamespace>());

        var method = (MethodDeclarationSyntax)generator.Generate(operation, null).Single();

        return method.Body!.Statements
            .Select(p => p.NormalizeWhitespace().ToFullString())
            .ToArray();
    }

    private sealed class StubSerializerSelector : ISerializerSelector
    {
        public SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<IOpenApiMediaType> mediaType) =>
            Qualities.TryGetValue(mediaType.Key, out double quality)
                ? new SerializerDescriptorWithPriority { Quality = quality }
                : null;
    }
}
