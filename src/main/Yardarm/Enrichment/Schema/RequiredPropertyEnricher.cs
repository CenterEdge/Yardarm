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

            if (!isRequired || context.Element.Nullable)
            {
                // If the property is optional OR nullable then the property should be nullable
                // This is because .NET does not have a good way to differentiate between missing and null
                syntax = syntax.MakeNullable();
            }
            else if (syntax.Parent is not InterfaceDeclarationSyntax)
            {
                // The value needs to be initialized to avoid nullable ref type warnings
                syntax = syntax.MakeNullableOrInitializeIfReferenceType(context.Compilation);
            }

            if (isRequired)
            {
                // Explicitly annotate as required if the schema requires the property
                // This must be done after any potential call to MakeNullableOrInitializeIfReferenceType to avoid errors
                syntax = AddRequiredAttribute(syntax);
            }

            return syntax;
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax) =>
            syntax
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
