using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Compilation
{
    internal class DefaultTypeSerializersEnricher : ICompilationEnricher
    {
        private readonly IList<ICreateDefaultRegistryEnricher> _createDefaultRegistryEnrichers;

        public Type[] ExecuteAfter { get; } =
        {
            typeof(SyntaxTreeCompilationEnricher)
        };

        public DefaultTypeSerializersEnricher(
            IEnumerable<ICreateDefaultRegistryEnricher> createDefaultRegistryEnrichers)
        {
            _createDefaultRegistryEnrichers = createDefaultRegistryEnrichers.ToArray();
        }

        public async ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target,
            CancellationToken cancellationToken = default)
        {
            foreach (SyntaxTree syntaxTree in target.SyntaxTrees)
            {
                SyntaxNode rootNode = await syntaxTree.GetRootAsync(cancellationToken)
                    .ConfigureAwait(false);

                ClassDeclarationSyntax? classDeclaration = rootNode
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(p => p.Identifier.ValueText == "TypeSerializerRegistry");

                MethodDeclarationSyntax? methodDeclaration = classDeclaration?
                    .ChildNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(p => p.Identifier.ValueText == "CreateDefaultRegistry");

                if (methodDeclaration?.ExpressionBody != null)
                {
                    MethodDeclarationSyntax newMethodDeclaration = methodDeclaration.WithExpressionBody(
                        methodDeclaration.ExpressionBody.WithExpression(
                            methodDeclaration.ExpressionBody.Expression.Enrich(_createDefaultRegistryEnrichers)));

                    rootNode = rootNode.ReplaceNode(methodDeclaration, newMethodDeclaration);

                    target = target.ReplaceSyntaxTree(syntaxTree,
                        syntaxTree.WithRootAndOptions(rootNode, syntaxTree.Options));
                }
            }

            return target;
        }
    }
}
