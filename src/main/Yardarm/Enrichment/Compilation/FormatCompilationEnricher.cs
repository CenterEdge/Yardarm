using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Extensions.Logging;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Compilation
{
    /// <summary>
    /// When <see cref="YardarmGenerationSettings.EmbedAllSources"/> is enabled, formats each
    /// <see cref="SyntaxTree"/> with proper whitespace formatting so that embedded source files
    /// are legible.
    /// </summary>
    /// <remarks>
    /// This does not run when not embedding source code for performance reasons.
    /// </remarks>
    public class FormatCompilationEnricher : ICompilationEnricher
    {
        private readonly YardarmGenerationSettings _settings;
        private readonly ILogger<FormatCompilationEnricher> _logger;

        public Type[] ExecuteAfter { get; } =
        {
            typeof(VersionAssemblyInfoEnricher),
            typeof(SyntaxTreeCompilationEnricher),
            typeof(DefaultTypeSerializersEnricher),
            typeof(OpenApiCompilationEnricher)
        };

        public FormatCompilationEnricher(YardarmGenerationSettings settings,
            ILogger<FormatCompilationEnricher> logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            _settings = settings;
            _logger = logger;
        }

        public async ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.EmbedAllSources)
            {
                // Don't bother formatting if we're not embedding source
                return target;
            }

            var stopwatch = Stopwatch.StartNew();

            using var workspace = new AdhocWorkspace();
            var solution = workspace
                .AddSolution(
                    SolutionInfo.Create(
                        SolutionId.CreateNewId(_settings.AssemblyName),
                        VersionStamp.Default));

            Project project =
                solution.AddProject(_settings.AssemblyName, _settings.AssemblyName + ".dll",
                    LanguageNames.CSharp);

            workspace.TryApplyChanges(solution);

            // Exclude resource files (already formatted) or files with no path (won't be embedded)
            IEnumerable<SyntaxTree> treesToBeFormatted = target.SyntaxTrees
                .Where(static p => p.FilePath != "" && p.HasCompilationUnitRoot &&
                                   p.GetCompilationUnitRoot().GetResourceNameAnnotation() is null);

            // Process formatting in parallel, this gives a slight perf boost
            object lockObj = new();
            await Parallel.ForEachAsync(treesToBeFormatted, cancellationToken,
                async (syntaxTree, localCt) =>
                {
                    SyntaxNode root = await syntaxTree.GetRootAsync(localCt);

                    Document document = project.AddDocument(Guid.NewGuid().ToString(), root);

                    document = await Formatter.FormatAsync(document, solution.Options, cancellationToken);

                    SyntaxNode? newRoot = await document.GetSyntaxRootAsync(localCt);

                    if (newRoot is not null && newRoot != root)
                    {
                        lock (lockObj)
                        {
                            target = target.ReplaceSyntaxTree(syntaxTree,
                                syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options));
                        }
                    }
                });

            stopwatch.Stop();

            _logger.LogInformation("Sources formatted for embedding in {elapsed}ms", stopwatch.ElapsedMilliseconds);

            return target;
        }
    }
}
