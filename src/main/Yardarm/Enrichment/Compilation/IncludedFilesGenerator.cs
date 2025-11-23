using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Compilation;

/// <summary>
/// Adds additional files to the compilation.
/// </summary>
/// <param name="generationContext"><see cref="GenerationContext"/>.</param>
public sealed class IncludedFilesGenerator(GenerationContext generationContext) : ISyntaxTreeGenerator
{
    /// <inheritdoc />
    public IEnumerable<SyntaxTree> Generate()
    {
        if (generationContext.Settings.IncludedFiles is not { Count: > 0 })
        {
            yield break;
        }

        foreach (IncludedFile includedFile in generationContext.Settings.IncludedFiles)
        {
            if (includedFile.ShouldInclude(generationContext))
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
                    includedFile.GetSourceText(),
                    CSharpParseOptions.Default
                        .WithLanguageVersion(LanguageVersion.CSharp14)
                        .WithPreprocessorSymbols(generationContext.PreprocessorSymbols),
                    path: PathHelpers.Combine(
                        generationContext.Settings.BasePath,
                        includedFile.SourceEmbeddingPath));

                // Annotate the compilation root so we know which included file it came from
                syntaxTree = syntaxTree.WithRootAndOptions(
                    syntaxTree.GetCompilationUnitRoot()
                        .AddIncludedFileNameAnnotation(includedFile.FilePath ?? includedFile.SourceEmbeddingPath),
                    syntaxTree.Options);

                yield return syntaxTree;
            }
        }
    }
}
