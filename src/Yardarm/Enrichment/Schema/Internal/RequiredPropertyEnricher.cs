using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class RequiredPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        private static readonly NameSyntax _requiredAttributeName =
            SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations.Required");

        private readonly GenerationContext _context;

        public int Priority => 0;

        public RequiredPropertyEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, LocatedOpenApiElement<OpenApiSchema> schema)
        {
            bool isRequired =
                schema.Parents.FirstOrDefault() is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
                parentSchema.Element.Required.Contains(schema.Key);

            return isRequired
                ? AddRequiredAttribute(syntax)
                : MakeNullable(syntax);
        }

        private PropertyDeclarationSyntax AddRequiredAttribute(PropertyDeclarationSyntax syntax)
        {
            var semanticModel = _context.Compilation.GetSemanticModel(syntax.SyntaxTree);

            var typeInfo = semanticModel.GetTypeInfo(syntax.Type);

            syntax = syntax.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                SyntaxFactory.Attribute(_requiredAttributeName)));

            if (typeInfo.Type?.IsReferenceType ?? false)
            {
                // Always mark reference types as nullable on schemas, even if they're required
                // This will encourage SDK consumers to check for nulls and prevent NREs

                syntax = MakeNullable(syntax);
            }

            return syntax;
        }

        private static PropertyDeclarationSyntax MakeNullable(PropertyDeclarationSyntax syntax) =>
            syntax.Type is NullableTypeSyntax
                ? syntax // Already nullable
                : syntax.WithType(
                    SyntaxFactory.NullableType(syntax.Type));
    }
}
