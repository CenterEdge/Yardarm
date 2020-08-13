using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Class;

        public ObjectSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()));

            declaration = AddProperties(declaration, Element, Schema.Properties);

            yield return declaration.Enrich(Context.Enrichers.Schema.Class, Element);
        }

        protected virtual ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax declaration,
            LocatedOpenApiElement<OpenApiSchema> parent, IEnumerable<KeyValuePair<string, OpenApiSchema>> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .SelectMany(p => DeclareProperty(parent.CreateChild(p.Value, p.Key), declaration.Identifier.ValueText))
                .ToArray();

            return declaration.AddMembers(members);
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> DeclareProperty(
            LocatedOpenApiElement<OpenApiSchema> property, string ownerName)
        {
            yield return CreatePropertyDeclaration(property, ownerName);

            if (property.Element.Reference == null)
            {
                // This isn't a reference, so we must generate the child schema

                ITypeGenerator generator = Context.SchemaGeneratorRegistry.Get(property);

                foreach (MemberDeclarationSyntax child in generator.Generate())
                {
                    yield return child;
                }
            }
        }

        protected virtual MemberDeclarationSyntax CreatePropertyDeclaration(LocatedOpenApiElement<OpenApiSchema> property, string ownerName)
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(property.Key);

            if (propertyName == ownerName)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeNameProvider.GetName(property);

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
