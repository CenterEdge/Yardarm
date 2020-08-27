using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Class;

        public ObjectSchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)TypeInfo.Name;

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()));

            declaration = AddProperties(declaration, Element.GetProperties());

            yield return declaration;
        }

        protected virtual ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax declaration,
            IEnumerable<ILocatedOpenApiElement<OpenApiSchema>> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .SelectMany(p => DeclareProperty(p, declaration.Identifier.ValueText))
                .ToArray();

            return declaration.AddMembers(members);
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> DeclareProperty(
            ILocatedOpenApiElement<OpenApiSchema> property, string ownerName)
        {
            yield return CreatePropertyDeclaration(property, ownerName);

            if (property.Element.Reference == null)
            {
                // This isn't a reference, so we must generate the child schema

                ITypeGenerator generator = Context.TypeGeneratorRegistry.Get(property);

                foreach (MemberDeclarationSyntax child in generator.Generate())
                {
                    yield return child;
                }
            }
        }

        protected virtual MemberDeclarationSyntax CreatePropertyDeclaration(ILocatedOpenApiElement<OpenApiSchema> property, string ownerName)
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(property.Key);

            if (propertyName == ownerName)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeGeneratorRegistry.Get(property).TypeInfo.Name;

            return SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddElementAnnotation(property, Context.ElementRegistry)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
    }
}
