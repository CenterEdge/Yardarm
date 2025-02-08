using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Compilation;

public class DefaultLiteralConvertersEnricher(
    [FromKeyedServices(DefaultLiteralConvertersEnricher.RegistrationEnricherKey)] IEnumerable<IRegistrationEnricher> createDefaultRegistryEnrichers)
    : IResourceFileEnricher
{
    public const string RegistrationEnricherKey = "DefaultLiteralConverters";

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
