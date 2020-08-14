using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequiredParameterEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiParameter>,
        IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiRequestBody>
    {
        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiParameter> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context.Compilation)
                : syntax.MakeNullable();
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiRequestBody> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context.Compilation)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax, CSharpCompilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

            var typeInfo = semanticModel.GetTypeInfo(syntax.Type);

            syntax = syntax.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                SyntaxFactory.Attribute(WellKnownTypes.RequiredAttribute())));

            if (typeInfo.Type?.IsReferenceType ?? false)
            {
                // Always mark reference types as nullable on schemas, even if they're required
                // This will encourage SDK consumers to check for nulls and prevent NREs

                syntax = syntax.MakeNullable();
            }

            return syntax;
        }
    }
}
