using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class HeaderDocumentationEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiHeader>
    {
        public int Priority => 100;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiHeader> context) =>
            string.IsNullOrWhiteSpace(context.Element.Description)
                ? target
                : AddDocumentation(target, context.Element);

        private PropertyDeclarationSyntax AddDocumentation(PropertyDeclarationSyntax target,
            OpenApiHeader context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0, GetDocumentationTrivia(context)));

        private SyntaxTrivia GetDocumentationTrivia(OpenApiHeader context) =>
            DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                DocumentationSyntaxHelpers.BuildSummaryElement(context.Description));
    }
}
