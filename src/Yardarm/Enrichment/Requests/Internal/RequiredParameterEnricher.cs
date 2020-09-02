using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequiredParameterEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiParameter>
    {
        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiParameter> context)
        {
            return context.Element.Required
                ? AddRequiredAttribute(syntax, context)
                : syntax.MakeNullable();
        }

        private PropertyDeclarationSyntax AddRequiredAttribute<T>(PropertyDeclarationSyntax syntax,
            OpenApiEnrichmentContext<T> context)
            where T : IOpenApiElement
        {
            bool forInterface = syntax.Parent is InterfaceDeclarationSyntax;

            if (!forInterface)
            {
                syntax = syntax.MakeNullableOrInitializeIfReferenceType(context.Compilation);
            }

            syntax = syntax
                .AddAttributeLists(AttributeList().AddAttributes(
                    Attribute(WellKnownTypes.System.ComponentModel.DataAnnotations.RequiredAttribute.Name)));

            return syntax;
        }
    }
}
