using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class OperationParameterDocumentationEnricher : IOperationParameterPropertyEnricher
    {
        public int Priority => 100;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiParameter> context) =>
            string.IsNullOrWhiteSpace(context.Element.Description)
                ? target
                : AddDocumentation(target, context.Element);

        private PropertyDeclarationSyntax AddDocumentation(PropertyDeclarationSyntax target,
            OpenApiParameter context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0, GetDocumentationTrivia(context)));

        private SyntaxTrivia GetDocumentationTrivia(OpenApiParameter context) =>
            DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                DocumentationSyntaxHelpers.BuildSummaryElement(context.Description));
    }
}
