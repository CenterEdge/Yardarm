using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests
{
    /// <summary>
    /// If the body is required, the body-less request class should never be instantiated. This enricher
    /// adds the abstract modifier to the class to prevent this.
    /// </summary>
    public class RequiredBodyRequestEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiOperation>
    {
        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            context.Element.RequestBody?.Required ?? false
                ? target.AddModifiers(Token(SyntaxKind.AbstractKeyword))
                : target;
    }
}
