using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Authentication
{
    /// <summary>
    /// Adds XML documentation to security scheme classes.
    /// </summary>
    public class SecuritySchemeDocumentationEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiSecurityScheme>
    {
        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSecurityScheme> context) =>
            !string.IsNullOrWhiteSpace(context.Element.Description)
                ? AddDocumentation(target, context.Element)
                : target;

        private ClassDeclarationSyntax AddDocumentation(ClassDeclarationSyntax target,
            IOpenApiSecurityScheme context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(
                        DocumentationSyntaxHelpers.BuildSummaryElement(context.Description))));
    }
}
