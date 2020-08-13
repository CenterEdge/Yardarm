using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequestClassMethodDocumentationEnricher : IOpenApiSyntaxNodeEnricher<MethodDeclarationSyntax, OpenApiOperation>
    {
        public int Priority => 100; // Run after most other enrichers

        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiOperation> context) =>
            target.Parent.IsKind(SyntaxKind.ClassDeclaration) && !string.IsNullOrWhiteSpace(context.Element.Summary)
                ? AddDocumentation(target)
                : target;


        private MethodDeclarationSyntax AddDocumentation(MethodDeclarationSyntax target) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                        DocumentationSyntaxHelpers.BuildInheritDocElement())));
    }
}
