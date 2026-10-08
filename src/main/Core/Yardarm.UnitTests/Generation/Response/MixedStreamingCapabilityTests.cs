using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Response;

/// <summary>
/// Tests responses which offer media types handled by serializers with different streaming support.
/// </summary>
public class MixedStreamingCapabilityTests
{
    private const string Accept = "requestMessage.Headers.Accept.Add(new global::System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(";

    // application/xml is handled by a serializer that reads List<T> but cannot stream
    private static string GetDocument(bool streaming) => $$"""
        {
          "openapi": "3.2.0",
          "info": { "title": "Test", "version": "1.0" },
          "paths": {
            "/things": {
              "get": {
                "operationId": "listThings",
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        {{(streaming ? "\"x-yardarm-streaming\": true," : "")}}
                        "schema": { "type": "array", "items": { "$ref": "#/components/schemas/Thing" } }
                      },
                      "application/jsonl": {
                        "itemSchema": { "$ref": "#/components/schemas/Thing" }
                      },
                      "application/xml": {
                        "schema": { "type": "array", "items": { "$ref": "#/components/schemas/Thing" } }
                      }
                    }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "Thing": { "type": "object", "properties": { "id": { "type": "integer" } } }
            }
          }
        }
        """;

    [Fact]
    public void GenerateAccept_StreamedBody_ExcludesMediaTypesWhichCannotStream()
    {
        var (operation, serviceProvider) = Load(streaming: true);

        string[] accept = GenerateAccept(operation, serviceProvider);

        accept.Should().Equal(
            $"{Accept}\"application/json\", 1));",
            $"{Accept}\"application/jsonl\", 0.8));");
    }

    [Fact]
    public void GenerateAccept_BufferedBody_IncludesAllCompatibleMediaTypes()
    {
        var (operation, serviceProvider) = Load(streaming: false);

        string[] accept = GenerateAccept(operation, serviceProvider);

        accept.Should().Equal(
            $"{Accept}\"application/json\", 1));",
            $"{Accept}\"application/jsonl\", 0.8));",
            $"{Accept}\"application/xml\", 0.5));");
    }

    [Fact]
    public void Resolve_StreamedBody_StreamsDespiteNonStreamingAlternative()
    {
        var (operation, serviceProvider) = Load(streaming: true);
        var response = operation.GetResponseSet().GetResponses().Single();

        var body = serviceProvider.GetRequiredService<IResponseBodyResolver>().Resolve(response);

        body!.IsStreaming.Should().BeTrue();
    }

    private static string[] GenerateAccept(ILocatedOpenApiElement<OpenApiOperation> operation,
        System.IServiceProvider serviceProvider)
    {
        var generator = serviceProvider.GetRequiredService<System.Collections.Generic.IEnumerable<IRequestMemberGenerator>>()
            .OfType<AddHeadersMethodGenerator>()
            .Single();

        var method = (MethodDeclarationSyntax)generator.Generate(operation, null).Single();

        return method.Body!.Statements
            .Select(p => p.NormalizeWhitespace().ToFullString())
            .Where(p => p.StartsWith(Accept))
            .ToArray();
    }

    private static (ILocatedOpenApiElement<OpenApiOperation> operation, System.IServiceProvider serviceProvider) Load(
        bool streaming)
    {
        OpenApiDocument document = OpenApiDocument.Parse(GetDocument(streaming), "json", new OpenApiReaderSettings()).Document;
        var serviceProvider = new YardarmGenerationSettings()
            .AddExtension<MixedStreamingTestExtension>()
            .BuildServiceProvider(document);

        return (document.Paths.ToLocatedElements().GetOperations().Single(), serviceProvider);
    }
}

/// <summary>
/// Registers streaming JSON and JSON Lines serializers alongside an XML serializer which cannot stream.
/// </summary>
public sealed class MixedStreamingTestExtension : YardarmExtension
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
        => services
            .AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/json", 1.0)),
                "Json",
                SyntaxFactory.ParseTypeName("global::Test.JsonTypeSerializer"),
                supportsStreaming: true))
            .AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/jsonl", 0.8)),
                "JsonLines",
                SyntaxFactory.ParseTypeName("global::Test.JsonLinesTypeSerializer"),
                supportsStreaming: true))
            .AddSerializerDescriptor(new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/xml", 0.5)),
                "Xml",
                SyntaxFactory.ParseTypeName("global::Test.XmlTypeSerializer")));
}
