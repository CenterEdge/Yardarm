using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Responses
{
    public class RequiredHeaderEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiHeader>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiHeader> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute<T>(PropertyDeclarationSyntax syntax,
            OpenApiEnrichmentContext<T> context)
            where T : IOpenApiElement =>
            syntax
                .MakeNullableOrInitializeIfReferenceType(context.Compilation)
                .AddAttributeLists(AttributeList().AddAttributes(
                    Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
