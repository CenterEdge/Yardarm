using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment.Compilation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.MicrosoftExtensionsHttp.Internal
{
    internal class ServiceCollectionExtensionsEnricher : IResourceFileEnricher
    {
        private readonly YardarmGenerationSettings _settings;

        public ServiceCollectionExtensionsEnricher(YardarmGenerationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _settings = settings;
        }

        public bool ShouldEnrich(string resourceName) =>
            resourceName == "Yardarm.MicrosoftExtensionsHttp.Client.ApiServiceCollectionExtensions.cs";

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
        {
            SyntaxToken newName = Identifier($"Add{_settings.AssemblyName.Replace(".", "")}Apis");

            var nodes = target
                .DescendantNodes(node => node is MemberDeclarationSyntax or CompilationUnitSyntax)
                .OfType<MethodDeclarationSyntax>()
                .Where(p => p.Identifier.ValueText == "AddApis");

            return target.ReplaceNodes(
                nodes,
                (_, node) => node.WithIdentifier(newName));
        }
    }
}
