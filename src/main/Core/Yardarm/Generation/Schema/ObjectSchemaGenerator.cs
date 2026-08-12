using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Class;

        public ObjectSchemaGenerator(ILocatedOpenApiElement<IOpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)TypeInfo.Name;

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(ConstructorDeclaration(className)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithBody(Block()));

            declaration = AddProperties(declaration, Element.GetProperties());

            if (Schema.AdditionalPropertiesAllowed)
            {
                declaration = declaration.AddMembers(
                    GenerateAdditionalPropertiesMember(Element.GetAdditionalPropertiesOrDefault()).ToArray());
            }

            yield return declaration;
        }

        protected virtual ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax declaration,
            IEnumerable<ILocatedOpenApiElement<IOpenApiSchema>> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .SelectMany(p => DeclareProperty(p, declaration.Identifier.ValueText))
                .ToArray();

            return declaration.AddMembers(members);
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> DeclareProperty(
            ILocatedOpenApiElement<IOpenApiSchema> property, string ownerName)
        {
            yield return CreatePropertyDeclaration(property, ownerName);

            if (property.Element is not IOpenApiReferenceHolder)
            {
                // This isn't a reference, so we must generate the child schema

                ITypeGenerator generator = Context.TypeGeneratorRegistry.Get(property);

                foreach (MemberDeclarationSyntax child in generator.Generate())
                {
                    yield return child;
                }
            }
        }

        protected virtual MemberDeclarationSyntax CreatePropertyDeclaration(ILocatedOpenApiElement<IOpenApiSchema> property, string ownerName)
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(property.Key);

            if (propertyName == ownerName)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeGeneratorRegistry.Get(property).TypeInfo.Name;

            return PropertyDeclaration(typeName, propertyName)
                .AddElementAnnotation(property, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateAdditionalPropertiesMember(
            ILocatedOpenApiElement<IOpenApiSchema> additionalProperties)
        {
            // The AdditionalProperties element isn't generated here, it's done by the AdditionalPropertiesEnricher
            // However, we do need to generate the schema now

            ITypeGenerator schemaGenerator = Context.TypeGeneratorRegistry.Get(additionalProperties);

            if (schemaGenerator.TypeInfo.IsGenerated && additionalProperties.Element is not IOpenApiReferenceHolder)
            {
                foreach (MemberDeclarationSyntax childTypeDeclaration in schemaGenerator.Generate())
                {
                    yield return childTypeDeclaration;
                }
            }
        }
    }
}
