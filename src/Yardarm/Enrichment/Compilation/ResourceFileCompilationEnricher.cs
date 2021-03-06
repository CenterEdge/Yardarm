using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Compilation
{
    /// <summary>
    /// Applies <see cref="IResourceFileEnricher"/> enrichment to any syntax trees which were derived from
    /// resource files.
    /// </summary>
    internal class ResourceFileCompilationEnricher : ICompilationEnricher
    {
        private readonly IResourceFileEnricher[] _enrichers;

        public Type[] ExecuteAfter { get; } =
        {
            typeof(SyntaxTreeCompilationEnricher)
        };

        public ResourceFileCompilationEnricher(IEnumerable<IResourceFileEnricher> enrichers)
        {
            _enrichers = enrichers.ToArray();
        }

        public ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target,
            CancellationToken cancellationToken = default) =>
            new ValueTask<CSharpCompilation>(
                _enrichers.Sort().Aggregate(target, Enrich));

        private CSharpCompilation Enrich(CSharpCompilation compilation, IResourceFileEnricher enricher)
        {
            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                var newSyntaxTree = syntaxTree;

                CompilationUnitSyntax compilationUnit = syntaxTree.GetCompilationUnitRoot();
                string? resourceName = compilationUnit.GetResourceNameAnnotation();

                if (resourceName != null && enricher.ShouldEnrich(resourceName))
                {
                    var context = new ResourceFileEnrichmentContext(compilation, syntaxTree, resourceName);

                    CompilationUnitSyntax newCompilationUnit = enricher.Enrich(compilationUnit, context);
                    if (newCompilationUnit != compilationUnit)
                    {
                        newSyntaxTree = syntaxTree.WithRootAndOptions(newCompilationUnit, syntaxTree.Options);
                    }
                }

                if (newSyntaxTree != syntaxTree)
                {
                    compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
                }
            }

            return compilation;
        }
    }
}
