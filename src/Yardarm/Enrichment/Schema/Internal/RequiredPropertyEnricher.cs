using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class RequiredPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            bool isRequired =
                context.LocatedElement.Parents.FirstOrDefault() is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
                parentSchema.Element.Required.Contains(context.LocatedElement.Key);

            return isRequired
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            syntax
                .MakeNullableOrInitializeIfReferenceType(context.Compilation)
                .AddAttributeLists(AttributeList().AddAttributes(
                    Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)));
    }
}
