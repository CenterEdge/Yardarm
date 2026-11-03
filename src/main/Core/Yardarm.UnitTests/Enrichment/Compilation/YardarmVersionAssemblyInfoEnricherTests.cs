using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Yardarm.Enrichment.Compilation;

namespace Yardarm.UnitTests.Enrichment.Compilation;

public class YardarmVersionAssemblyInfoEnricherTests
{
    [Fact]
    public void YardarmVersion_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(YardarmVersionAssemblyInfoEnricher.YardarmVersion));
    }

    [Fact]
    public void Enrich_EmptyCompilationUnit_AddsAssemblyMetadataAttribute()
    {
        var enricher = new YardarmVersionAssemblyInfoEnricher();

        CompilationUnitSyntax result = enricher.Enrich(SyntaxFactory.CompilationUnit());

        Assert.Equal(
            $"[assembly: System.Reflection.AssemblyMetadata(\"YardarmVersion\", \"{YardarmVersionAssemblyInfoEnricher.YardarmVersion}\")]",
            result.NormalizeWhitespace().ToFullString().ReplaceLineEndings("\n").Trim());
    }
}
