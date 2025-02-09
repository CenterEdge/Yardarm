using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Enrichment.Registration;

namespace Yardarm.NewtonsoftJson.Internal;

/// <summary>
/// Enrich the default JsonSerializerSettings in the JsonTypeSerializer class with additional settings
/// from extensions registered as <see cref="IRegistrationEnricher"/> with the key <c>JsonSerializerSettings</c>.
/// </summary>
internal class JsonSerializerSettingsEnricher(
    [FromKeyedServices(JsonSerializerSettingsEnricher.RegistrationEnricherKey)] IEnumerable<IRegistrationEnricher> enrichers)
    : IResourceFileEnricher
{
    public const string RegistrationEnricherKey = "JsonSerializerSettings";

    public bool ShouldEnrich(string resourceName) =>
        resourceName == "Yardarm.NewtonsoftJson.Client.Serialization.Json.JsonTypeSerializer.cs";

    public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
    {
        ClassDeclarationSyntax? classDeclaration = target
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "JsonTypeSerializer");

        MethodDeclarationSyntax? methodDeclaration = classDeclaration?
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "CreateDefaultSettings");

        if (methodDeclaration?.Body is { } body)
        {
            MethodDeclarationSyntax newMethodDeclaration = methodDeclaration.WithBody(
                body.Enrich(enrichers));

            target = target.ReplaceNode(methodDeclaration, newMethodDeclaration);
        }

        return target;
    }
}
