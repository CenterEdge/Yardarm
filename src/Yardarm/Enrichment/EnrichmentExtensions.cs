using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public static class EnrichmentExtensions
    {
        private static readonly MethodInfo _genericEnrichCompilationMethod = typeof(EnrichmentExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(p => p.Name == nameof(Enrich) && p.IsGenericMethodDefinition &&
                         p.GetParameters()[0].ParameterType == typeof(CSharpCompilation));

        public static TTarget Enrich<TTarget>(this TTarget target, IEnumerable<IEnricher<TTarget>> enrichers) =>
            enrichers
                .OrderBy(p => p.Priority)
                .Aggregate(target, (p, enricher) => enricher.Enrich(p));

        public static TTarget Enrich<TTarget, TContext>(this TTarget target, IEnumerable<IEnricher<TTarget, TContext>> enrichers, TContext context) =>
            enrichers
                .OrderBy(p => p.Priority)
                .Aggregate(target, (p, enricher) => enricher.Enrich(p, context));

        public static SyntaxTree Enrich<TSyntaxNode, TElement>(this SyntaxTree syntaxTree,
            IEnricher<TSyntaxNode, LocatedOpenApiElement<TElement>> enricher,
            GenerationContext context)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiSerializable
        {
            var rootNode = syntaxTree.GetRoot();

            var newRootNode = rootNode.ReplaceNodes(
                rootNode.GetAnnotatedNodes(typeof(TElement).Name).OfType<TSyntaxNode>(),
                (_, node) =>
                {
                    var schema = node.GetElementAnnotation<TElement>(context.ElementRegistry);

                    return schema != null
                        ? enricher.Enrich(node, schema)
                        : node;
                });

            return rootNode != newRootNode
                ? syntaxTree.WithRootAndOptions(newRootNode, syntaxTree.Options)
                : syntaxTree;
        }

        public static CSharpCompilation Enrich<TSyntaxNode, TElement>(this CSharpCompilation compilation,
            IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> enricher,
            GenerationContext context)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiSerializable
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                SyntaxTree newSyntaxTree = syntaxTree.Enrich(enricher, context);
                if (syntaxTree != newSyntaxTree)
                {
                    compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
                }
            }

            return compilation;
        }

        public static CSharpCompilation Enrich(this CSharpCompilation compilation,
            IOpenApiSyntaxNodeEnricher enricher,
            GenerationContext context)
        {
            foreach (Type interfaceType in enricher.GetType().GetInterfaces()
                .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IOpenApiSyntaxNodeEnricher<,>)))
            {
                var enrichMethod =
                    _genericEnrichCompilationMethod.MakeGenericMethod(interfaceType.GetGenericArguments());

                compilation = (CSharpCompilation)enrichMethod.Invoke(null, new object[] {compilation, enricher, context})!;
            }

            return compilation;
        }
    }
}
