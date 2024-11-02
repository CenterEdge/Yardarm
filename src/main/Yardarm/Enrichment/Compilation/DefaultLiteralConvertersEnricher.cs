using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Compilation;

public class DefaultLiteralConvertersEnricher(
    IEnumerable<IDefaultLiteralConverterEnricher> createDefaultRegistryEnrichers)
    : IResourceFileEnricher
{
    public bool ShouldEnrich(string resourceName) =>
        resourceName == "Yardarm.Client.Serialization.Literals.LiteralConverterRegistry.cs";

    public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
    {
        ClassDeclarationSyntax? classDeclaration = target
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "LiteralConverterRegistry");

        MethodDeclarationSyntax? methodDeclaration = classDeclaration?
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "CreateDefaultRegistry");

        if (methodDeclaration?.ExpressionBody != null)
        {
            MethodDeclarationSyntax newMethodDeclaration = methodDeclaration.WithExpressionBody(
                methodDeclaration.ExpressionBody.WithExpression(
                    methodDeclaration.ExpressionBody.Expression.Enrich(createDefaultRegistryEnrichers)));

            target = target.ReplaceNode(methodDeclaration, newMethodDeclaration);
        }

        return target;
    }
}
