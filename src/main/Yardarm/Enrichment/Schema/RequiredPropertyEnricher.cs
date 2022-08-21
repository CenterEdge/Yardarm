using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Schema
{
    public class RequiredPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            bool isRequired =
                context.LocatedElement.Parent is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
                parentSchema.Element.Required.Contains(context.LocatedElement.Key);

            return isRequired
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var newSyntax = context.Element.Nullable
                // If the schema is nullable the property should be nullable
                ? syntax.MakeNullable()
                // If the schema is not nullable we should try to initialize it, fallback to making it nullable if we can't
                : syntax.MakeNullableOrInitializeIfReferenceType(context.Compilation);

            return newSyntax
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
