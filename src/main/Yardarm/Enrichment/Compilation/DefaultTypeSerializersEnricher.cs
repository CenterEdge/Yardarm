using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Registration;

namespace Yardarm.Enrichment.Compilation;

public class DefaultTypeSerializersEnricher(
    [FromKeyedServices(DefaultTypeSerializersEnricher.RegistrationEnricherKey)] IEnumerable<IRegistrationEnricher> createDefaultRegistryEnrichers) :
    IResourceFileEnricher
{
    public const string RegistrationEnricherKey = "DefaultTypeSerializers";

    public bool ShouldEnrich(string resourceName) =>
        resourceName == "Yardarm.Client.Serialization.TypeSerializerRegistry.cs";

    public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
    {
        ClassDeclarationSyntax? classDeclaration = target
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "TypeSerializerRegistry");

        MethodDeclarationSyntax? methodDeclaration = classDeclaration?
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "CreateDefaultRegistry");

        if (methodDeclaration?.Body is { } body)
        {
            MethodDeclarationSyntax newMethodDeclaration = methodDeclaration.WithBody(
                body.Enrich(createDefaultRegistryEnrichers));

            target = target.ReplaceNode(methodDeclaration, newMethodDeclaration);
        }

        return target;
    }
}
