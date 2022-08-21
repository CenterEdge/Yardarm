using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests
{
    public class RequestParameterDocumentationEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiParameter>
    {
        public Type[] ExecuteAfter { get; } =
        {
            typeof(RequiredParameterEnricher),
            typeof(RequestInterfaceMethodDocumentationEnricher),
            typeof(RequestClassMethodDocumentationEnricher)
        };

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiParameter> context) =>
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
