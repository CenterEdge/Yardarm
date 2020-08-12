using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class RequiredPropertyEnricher : ISchemaPropertyEnricher
    {
        public static readonly NameSyntax RequiredAttributeName =
            SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations.Required");

        public int Priority => 0;

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax,
            LocatedOpenApiElement<OpenApiSchema> property) =>
            property.Parents.FirstOrDefault() is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
            parentSchema.Element.Required.Contains(property.Key)
                ? AddRequiredAttribute(syntax)
                : MakeNullable(syntax);

        private static PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax) =>
            syntax.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                SyntaxFactory.Attribute(RequiredAttributeName)));

        private static PropertyDeclarationSyntax MakeNullable(PropertyDeclarationSyntax syntax) =>
            syntax.Type is NullableTypeSyntax
                ? syntax // Already nullable
                : syntax.WithType(
                    SyntaxFactory.NullableType(syntax.Type));
    }
}
