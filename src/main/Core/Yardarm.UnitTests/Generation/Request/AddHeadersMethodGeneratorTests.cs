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
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
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

    private const string ThingArray = """{ "type": "array", "items": { "$ref": "#/components/schemas/Thing" } }""";
    private const string ThingObject = """{ "$ref": "#/components/schemas/Thing" }""";

    private static string ThingResponse(string jsonSchema) => $$"""
        "200": {
          "description": "OK",
          "content": {
            "application/json": { "schema": {{jsonSchema}} },
            "application/jsonl": { "itemSchema": { "$ref": "#/components/schemas/Thing" } }
          }
        }
        """;

    [Fact]
    public void Generate_JsonArrayAndJsonLinesSameBodyType_AcceptsBoth()
    {
        string[] result = GenerateAccept(ThingResponse(ThingArray));

        result.Should().Equal(
            $"{Accept}\"application/json\", 1));",
            $"{Accept}\"application/jsonl\", 0.95));");
    }

    [Fact]
    public void Generate_JsonObjectAndJsonLinesDifferentBodyType_AcceptsOnlySelected()
    {
        string[] result = GenerateAccept(ThingResponse(ThingObject));

        result.Should().Equal($"{Accept}\"application/json\", 1));");
    }

    [Fact]
    public void GenerateGetBody_JsonArrayAndJsonLinesSameBodyType_DeserializesSequence()
    {
        string result = GenerateGetBody(ThingResponse(ThingArray));

        result.Should().Contain("DeserializeSequenceAsync<").And.NotContain("DeserializeAsync<");
    }

    [Fact]
    public void GenerateGetBody_JsonObjectAndJsonLinesDifferentBodyType_Deserializes()
    {
        string result = GenerateGetBody(ThingResponse(ThingObject));

        result.Should().Contain("DeserializeAsync<").And.NotContain("DeserializeSequenceAsync<");
    }

    private static (OpenApiDocument Document, IServiceProvider ServiceProvider, PriorityMediaTypeSelector Selector, ILocatedOpenApiElement<OpenApiOperation> Operation) Load(
        string responses)
    {
        string documentText = $$"""
            {
              "openapi": "3.2.0",
              "info": { "title": "Test", "version": "1.0" },
              "components": { "schemas": { "Thing": { "type": "object", "properties": { "id": { "type": "integer" } } } } },
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

        return (document, serviceProvider, new PriorityMediaTypeSelector(new StubSerializerSelector()), operation);
    }

    private static string[] GenerateAccept(string responses)
    {
        var (_, serviceProvider, selector, operation) = Load(responses);

        var generator = new AddHeadersMethodGenerator(
            serviceProvider.GetRequiredService<IRequestsNamespace>(),
            selector,
            new StubSerializerSelector(),
            serviceProvider.GetRequiredService<GenerationContext>(),
            serviceProvider.GetRequiredService<INameFormatterSelector>(),
            serviceProvider.GetRequiredService<ISerializationNamespace>());

        var method = (MethodDeclarationSyntax)generator.Generate(operation, null).Single();

        return method.Body!.Statements
            .Select(p => p.NormalizeWhitespace().ToFullString())
            .ToArray();
    }

    private static string GenerateGetBody(string responses)
    {
        var (_, serviceProvider, selector, operation) = Load(responses);

        var serializerSelector = new StubSerializerSelector();
        var context = serviceProvider.GetRequiredService<GenerationContext>();

        var generator = new GetBodyMethodGenerator(
            selector,
            serializerSelector,
            context,
            serviceProvider.GetRequiredService<ISerializationNamespace>(),
            new DefaultResponseBodyResolver(selector, serializerSelector, context));

        var response = operation.GetResponseSet().GetResponses().Single();

        return generator.Generate(response, "GetThingsResponse").Single().NormalizeWhitespace().ToFullString();
    }

    private sealed class StubSerializerSelector : ISerializerSelector
    {
        public SerializerDescriptorWithPriority? Select(ILocatedOpenApiElement<IOpenApiMediaType> mediaType) =>
            Qualities.TryGetValue(mediaType.Key, out double quality)
                ? new SerializerDescriptorWithPriority { Quality = quality }
                : null;
    }
}
