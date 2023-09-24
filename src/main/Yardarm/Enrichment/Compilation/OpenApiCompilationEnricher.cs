using System;
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

        public Type[] ExecuteAfter { get; } =
        {
            typeof(DefaultTypeSerializersEnricher)
        };

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
            new ValueTask<CSharpCompilation>(
                _enrichers.Sort().Aggregate(target, Enrich));

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
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                SyntaxTree newSyntaxTree = Enrich(syntaxTree, compilation, enricher);
                if (syntaxTree != newSyntaxTree)
                {
                    compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
                }
            }

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
