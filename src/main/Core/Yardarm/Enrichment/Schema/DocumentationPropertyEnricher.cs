using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Schema
{
    public class DocumentationPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, IOpenApiSchema>
    {
        public Type[] ExecuteAfter { get; } =
        {
            typeof(AdditionalPropertiesEnricher),
            typeof(RequiredPropertyEnricher)
        };

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context) =>
            string.IsNullOrWhiteSpace(context.Element.Description)
                ? target
                : AddDocumentation(target, context.Element);

        private PropertyDeclarationSyntax AddDocumentation(PropertyDeclarationSyntax target,
            IOpenApiSchema context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0, GetDocumentationTrivia(context)));

        private SyntaxTrivia GetDocumentationTrivia(IOpenApiSchema context) =>
            DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                DocumentationSyntaxHelpers.BuildSummaryElement(context.Description ?? string.Empty));
    }
}
