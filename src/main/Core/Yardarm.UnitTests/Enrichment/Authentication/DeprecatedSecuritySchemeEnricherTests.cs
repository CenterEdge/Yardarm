using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Xunit;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Authentication;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Enrichment.Authentication;

public class DeprecatedSecuritySchemeEnricherTests
{
    [Fact]
    public void Enrich_DeprecatedSecurityScheme_AddsObsoleteAttribute()
    {
        var securityScheme = ReadSecurityScheme("deprecated: true");
        var target = ParseClassDeclaration();

        ClassDeclarationSyntax result = Enrich(target, securityScheme);

        Assert.Equal(
            "[global::System.ObsoleteAttribute(\"Security scheme legacyAuth has been marked deprecated.\")]\n"
            + "public class LegacyAuth\n"
            + "{\n"
            + "}",
            result.NormalizeWhitespace().ToFullString().ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Enrich_NonDeprecatedSecurityScheme_DoesNotAddObsoleteAttribute()
    {
        var securityScheme = ReadSecurityScheme();
        var target = ParseClassDeclaration();

        ClassDeclarationSyntax result = Enrich(target, securityScheme);

        Assert.Empty(result.AttributeLists);
    }

    private static ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target, IOpenApiSecurityScheme securityScheme)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(target.SyntaxTree.GetText());
        var compilation = CSharpCompilation.Create("Test", [syntaxTree]);
        var context = new OpenApiEnrichmentContext<IOpenApiSecurityScheme>(
            compilation, syntaxTree, securityScheme.CreateRoot("legacyAuth"), target);

        return new DeprecatedSecuritySchemeEnricher().Enrich(target, context);
    }

    private static ClassDeclarationSyntax ParseClassDeclaration() =>
        SyntaxFactory.ParseCompilationUnit("public class LegacyAuth { }")
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

    private static IOpenApiSecurityScheme ReadSecurityScheme(string deprecated = "")
        => ReadDocument(deprecated).Components!.SecuritySchemes!["legacyAuth"];

    private static OpenApiDocument ReadDocument(string deprecated = "")
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
