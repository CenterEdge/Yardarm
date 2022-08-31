using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests
{
    public class RequiredParameterEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiParameter>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiParameter> context)
        {
            if (!context.Element.Required || context.Element.Schema.Nullable)
            {
                // If the parameter is optional OR nullable then the property should be nullable
                // This is because .NET does not have a good way to differentiate between missing and null
                syntax = syntax.MakeNullable();
            }
            else
            {
                // The value needs to be initialized to avoid nullable ref type warnings
                syntax = syntax.MakeNullableOrInitializeIfReferenceType(context.Compilation);
            }

            if (context.Element.Required)
            {
                // Explicitly annotate as required if the parameter is required
                // This must be done after any potential call to MakeNullableOrInitializeIfReferenceType to avoid errors
                syntax = AddRequiredAttribute(syntax);
            }

            return syntax;
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax) =>
            syntax.AddAttributeLists(
                AttributeList(SingletonSeparatedList(
                        Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
