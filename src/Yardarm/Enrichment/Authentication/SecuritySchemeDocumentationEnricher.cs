using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Authentication
{
    /// <summary>
    /// Adds XML documentation to security scheme classes.
    /// </summary>
    public class SecuritySchemeDocumentationEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSecurityScheme>
    {
        public int Priority => 0;

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSecurityScheme> context) =>
            !string.IsNullOrWhiteSpace(context.Element.Description)
                ? AddDocumentation(target, context.Element)
                : target;

        private ClassDeclarationSyntax AddDocumentation(ClassDeclarationSyntax target,
            OpenApiSecurityScheme context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                        DocumentationSyntaxHelpers.BuildSummaryElement(context.Description))));
    }
}
