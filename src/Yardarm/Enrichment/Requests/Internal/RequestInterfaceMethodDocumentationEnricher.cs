using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequestInterfaceMethodDocumentationEnricher : IOpenApiSyntaxNodeEnricher<MethodDeclarationSyntax, OpenApiOperation>
    {
        public int Priority => 100; // Run after most other enrichers

        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            target.Parent.IsKind(SyntaxKind.InterfaceDeclaration) && !string.IsNullOrWhiteSpace(context.Element.Summary)
                ? AddDocumentation(target, context.Element)
                : target;

        private MethodDeclarationSyntax AddDocumentation(MethodDeclarationSyntax target,
            OpenApiOperation context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(GetSections(context).ToArray<XmlNodeSyntax>())));

        private IEnumerable<XmlElementSyntax> GetSections(OpenApiOperation context)
        {
            yield return DocumentationSyntaxHelpers.BuildSummaryElement(context.Summary);

            if (!string.IsNullOrWhiteSpace(context.Description))
            {
                yield return DocumentationSyntaxHelpers.BuildRemarksElement(context.Description);
            }
        }

    }
}
