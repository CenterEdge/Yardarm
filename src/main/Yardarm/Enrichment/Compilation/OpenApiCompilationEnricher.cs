using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Compilation
{
    public class OpenApiCompilationEnricher : ICompilationEnricher
    {
        private static MethodInfo? s_genericEnrichCompilationMethod;

        private readonly IOpenApiElementRegistry _elementRegistry;
        private readonly IList<IOpenApiSyntaxNodeEnricher> _enrichers;

        // Pool these for reuse by each enricher
        private readonly ConcurrentBag<SyntaxTree> _toRemove = [];
        private readonly ConcurrentBag<SyntaxTree> _toAdd = [];

        public Type[] ExecuteAfter { get; } =
        [
            typeof(ResourceFileCompilationEnricher),
        ];

        public OpenApiCompilationEnricher(IOpenApiElementRegistry elementRegistry,
            IEnumerable<IOpenApiSyntaxNodeEnricher> enrichers)
        {
            ArgumentNullException.ThrowIfNull(elementRegistry);
            ArgumentNullException.ThrowIfNull(enrichers);

            _elementRegistry = elementRegistry;
            _enrichers = enrichers.ToArray();
        }

        public ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target,
            CancellationToken cancellationToken = default) =>
            new(_enrichers.Sort().Aggregate(target, Enrich));

        private CSharpCompilation Enrich(CSharpCompilation compilation, IOpenApiSyntaxNodeEnricher enricher)
        {
            var genericEnrichCompilationMethod = s_genericEnrichCompilationMethod ??=
                ((Func<CSharpCompilation, IOpenApiSyntaxNodeEnricher<SyntaxNode, OpenApiSchema>, CSharpCompilation>)Enrich)
                    .GetMethodInfo().GetGenericMethodDefinition();

            foreach (Type interfaceType in enricher.GetType().GetInterfaces()
                .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IOpenApiSyntaxNodeEnricher<,>)))
            {
                var enrichMethod =
                    genericEnrichCompilationMethod.MakeGenericMethod(interfaceType.GetGenericArguments());

                compilation = (CSharpCompilation)enrichMethod.Invoke(this, new object[] {compilation, enricher})!;
            }

            return compilation;
        }

        private CSharpCompilation Enrich<TSyntaxNode, TElement>(CSharpCompilation compilation,
            IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> enricher)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiElement
        {
            // Execute enrichment on syntax trees in parallel to allow the use of multiple CPU cores
            // This means that the CSharpCompilation passed to the enricher for each syntax tree is
            // the same instance and won't include mutations on any other syntax tree. However, each
            // enricher is still run in sequence and will include mutations made by other enrichers.
            Parallel.ForEach(compilation.SyntaxTrees, syntaxTree =>
            {
                SyntaxTree newSyntaxTree = Enrich(syntaxTree, compilation, enricher);
                if (syntaxTree != newSyntaxTree)
                {
                    _toRemove.Add(syntaxTree);
                    _toAdd.Add(newSyntaxTree);
                }
            });

            if (!_toRemove.IsEmpty)
            {
                compilation = compilation
                    .RemoveSyntaxTrees(_toRemove)
                    .AddSyntaxTrees(_toAdd);
            }

            // Clear for the next enricher
            _toRemove.Clear();
            _toAdd.Clear();

            return compilation;
        }

        private SyntaxTree Enrich<TSyntaxNode, TElement>(SyntaxTree syntaxTree,
            CSharpCompilation compilation,
            IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> enricher)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiElement
        {
            var rootNode = syntaxTree.GetRoot();

            var newRootNode = rootNode.ReplaceNodes(
                rootNode.GetAnnotatedNodes(typeof(TElement).Name).OfType<TSyntaxNode>(),
                (originalNode, node) =>
                {
                    var schema = node.GetElementAnnotation<TElement>(_elementRegistry);

                    return schema != null
                        ? enricher.Enrich(node, new OpenApiEnrichmentContext<TElement>(compilation, syntaxTree, schema, originalNode))
                        : node;
                });

            return rootNode != newRootNode
                ? syntaxTree.WithRootAndOptions(newRootNode, syntaxTree.Options)
                : syntaxTree;
        }
    }
}
