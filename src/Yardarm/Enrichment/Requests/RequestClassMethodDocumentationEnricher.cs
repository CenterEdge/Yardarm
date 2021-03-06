using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests
{
    public class RequestClassMethodDocumentationEnricher : IOpenApiSyntaxNodeEnricher<MethodDeclarationSyntax, OpenApiOperation>
    {
        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            target.Parent.IsKind(SyntaxKind.ClassDeclaration) && !string.IsNullOrWhiteSpace(context.Element.Summary)
                ? AddDocumentation(target)
                : target;

        private MethodDeclarationSyntax AddDocumentation(MethodDeclarationSyntax target) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0, DocumentationSyntaxHelpers.InheritDocTrivia));
    }
}
