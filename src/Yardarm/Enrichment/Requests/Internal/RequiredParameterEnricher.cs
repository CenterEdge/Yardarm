using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequiredParameterEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiParameter>,
        IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiRequestBody>
    {
        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiParameter> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiRequestBody> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute<T>(PropertyDeclarationSyntax syntax,
            OpenApiEnrichmentContext<T> context)
            where T : IOpenApiSerializable =>
            syntax
                .MakeNullableOrInitializeIfReferenceType(context.Compilation)
                .AddAttributeLists(AttributeList().AddAttributes(
                    Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)));
    }
}
