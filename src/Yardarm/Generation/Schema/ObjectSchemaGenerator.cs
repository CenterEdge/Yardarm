using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : SchemaGeneratorBase
    {
        protected IList<ISchemaClassEnricher> ClassEnrichers { get; }
        protected IList<IPropertyEnricher> PropertyEnrichers { get; }

        protected override NameKind NameKind => NameKind.Class;

        public ObjectSchemaGenerator(INamespaceProvider namespaceProvider, ITypeNameGenerator typeNameGenerator,
            INameFormatterSelector nameFormatterSelector, ISchemaGeneratorFactory schemaGeneratorFactory,
            IEnumerable<ISchemaClassEnricher> classEnrichers, IEnumerable<IPropertyEnricher> propertyEnrichers)
            : base(namespaceProvider, typeNameGenerator, nameFormatterSelector, schemaGeneratorFactory)
        {
            ClassEnrichers = classEnrichers.ToArray();
            PropertyEnrichers = propertyEnrichers.ToArray();
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate(LocatedOpenApiElement<OpenApiSchema> element)
        {
            OpenApiSchema schema = element.Element;

            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName(element);

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()));

            declaration = AddProperties(declaration, element, schema.Properties);

            yield return declaration.Enrich(ClassEnrichers, element);
        }

        protected virtual ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax declaration,
            LocatedOpenApiElement<OpenApiSchema> parent, IEnumerable<KeyValuePair<string, OpenApiSchema>> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .SelectMany(p => DeclareProperty(parent.CreateChild(p.Value, p.Key)))
                .ToArray();

            return declaration.AddMembers(members);
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> DeclareProperty(
            LocatedOpenApiElement<OpenApiSchema> property)
        {
            yield return CreatePropertyDeclaration(property);

            if (property.Element.Reference == null)
            {
                // This isn't a reference, so we must generate the child schema

                ISchemaGenerator generator = SchemaGeneratorFactory.Get(property);

                foreach (MemberDeclarationSyntax child in generator.Generate(property))
                {
                    yield return child;
                }
            }
        }

        protected virtual MemberDeclarationSyntax CreatePropertyDeclaration(LocatedOpenApiElement<OpenApiSchema> property)
        {
            string propertyName = NameFormatterSelector.GetFormatter(NameKind.Property).Format(property.Key);

            var typeName = TypeNameGenerator.GetName(property);

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return propertyDeclaration.Enrich(PropertyEnrichers, property);
        }
    }
}
