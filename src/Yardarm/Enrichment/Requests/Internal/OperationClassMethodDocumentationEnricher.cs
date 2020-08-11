using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class OperationClassMethodDocumentationEnricher : IOperationClassMethodEnricher
    {
        public int Priority => 100; // Run after most other enrichers

        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiOperation> context) =>
            string.IsNullOrWhiteSpace(context.Element.Summary)
                ? target
                : AddDocumentation(target, context.Element);

        private MethodDeclarationSyntax AddDocumentation(MethodDeclarationSyntax target,
            OpenApiOperation context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                        DocumentationSyntaxHelpers.BuildInheritDocElement())));
    }
}
