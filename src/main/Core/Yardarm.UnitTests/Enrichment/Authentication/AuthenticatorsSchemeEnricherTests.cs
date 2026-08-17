using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Enrichment.Authentication;

namespace Yardarm.UnitTests.Enrichment.Authentication;

public class AuthenticatorsSchemeEnricherTests
{
    [Theory]
    [InlineData("deprecated: true", 1)]
    [InlineData("", 0)]
    public void GenerateProperties_SecurityScheme_DeprecationIsAppliedToProperty(
        string deprecated, int expectedAttributeCount)
    {
        var document = ReadDocument(deprecated);
        var serviceProvider = new YardarmGenerationSettings().BuildServiceProvider(document);
        var target = new AuthenticatorsSchemeEnricher(
            serviceProvider.GetRequiredService<GenerationContext>());

        PropertyDeclarationSyntax property = target.GenerateProperties().Single();

        Assert.Equal(expectedAttributeCount, property.AttributeLists.Count);
    }

    private static OpenApiDocument ReadDocument(string deprecated)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
              openapi: 3.2.0
              info:
                title: Test
                version: 1.0.0
              components:
                securitySchemes:
                  legacyAuth:
                    type: http
                    scheme: bearer
                    {{deprecated}}
              """));

        return OpenApiDocument.LoadAsync(stream, settings: settings).GetAwaiter().GetResult().Document;
    }
}
